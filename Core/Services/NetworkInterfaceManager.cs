using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FakeHostLocalLab.Core.Models;

namespace FakeHostLocalLab.Core.Services;

public static class NetworkInterfaceManager
{
    // Prevents concurrent SyncIps / RemoveAllIps calls from racing each other.
    private static readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);
    private const string LabSwitchName = "LIHT-Net";

    public static string EnsureLabInterfaceExists()
    {
        LogBus.Log($"Checking for Dedicated Switch: {LabSwitchName}");
        
        var checkOutput = RunPowerShell($"Get-VMSwitch -Name '{LabSwitchName}' -ErrorAction SilentlyContinue");
        
        if (string.IsNullOrWhiteSpace(checkOutput))
        {
            LogBus.Log($"Switch '{LabSwitchName}' not found. Creating Internal Hyper-V Switch...");
            var createResult = RunPowerShell($"New-VMSwitch -Name '{LabSwitchName}' -SwitchType Internal -Notes 'Used by LIHT' -ErrorAction Stop");
            
            if (string.IsNullOrEmpty(createResult))
            {
                LogBus.Log("ERROR: Failed to create Internal Switch. Check Hyper-V permissions.");
                return string.Empty;
            }
            LogBus.Log("Switch created successfully.");
        }

        var aliasOutput = RunPowerShell($"Get-NetAdapter | Where-Object {{ $_.Name -like '*({LabSwitchName})*' -or $_.InterfaceDescription -like '*({LabSwitchName})*' }} | Select-Object -ExpandProperty Name | Select-Object -First 1");
        var alias = aliasOutput.Trim();

        if (string.IsNullOrEmpty(alias))
        {
            LogBus.Log("Could not identify the network adapter for LIHT-Net.");
            return string.Empty;
        }

        LogBus.Log($"Using Interface: {alias}");
        return alias;
    }

    private static string GetExistingAlias()
    {
        var aliasOutput = RunPowerShell($"Get-NetAdapter | Where-Object {{ $_.Name -like '*({LabSwitchName})*' -or $_.InterfaceDescription -like '*({LabSwitchName})*' }} | Select-Object -ExpandProperty Name | Select-Object -First 1");
        return aliasOutput.Trim();
    }

    public static void RemoveAllIps(AppConfig config)
    {
        _syncLock.Wait();
        try
        {
            var alias = GetExistingAlias();
            if (string.IsNullOrEmpty(alias))
            {
                LogBus.Log("No LIHT-Net adapter found. Nothing to clean.");
                return;
            }

            var baseIpPart = ExtractBaseIpPart(config.BaseNetwork);
            var currentIps = GetCurrentIps(alias, baseIpPart);

            foreach (var ip in currentIps)
            {
                LogBus.Log($"Removing IP {ip} from {alias}...");
                RunPowerShell($"Remove-NetIPAddress -InterfaceAlias '{alias}' -IPAddress '{ip}' -Confirm:$false -ErrorAction SilentlyContinue");
            }
        }
        finally { _syncLock.Release(); }
    }

    public static void RemoveLabSwitch()
    {
        LogBus.Log($"Attempting to remove Dedicated Switch: {LabSwitchName}...");
        var checkOutput = RunPowerShell($"Get-VMSwitch -Name '{LabSwitchName}' -ErrorAction SilentlyContinue");
        if (string.IsNullOrWhiteSpace(checkOutput))
        {
            LogBus.Log("Switch not found. Nothing to remove.");
            return;
        }

        RunPowerShell($"Remove-VMSwitch -Name '{LabSwitchName}' -Force -ErrorAction Stop");
        LogBus.Log("Switch removed successfully.");
    }

    public static void SyncIps(AppConfig config)
    {
        _syncLock.Wait();
        try
        {
            int prefixLength = ExtractPrefixLength(config.BaseNetwork);
            var alias = EnsureLabInterfaceExists();

            if (string.IsNullOrEmpty(alias)) return;

            var baseIpPart = ExtractBaseIpPart(config.BaseNetwork);
            var currentIps = GetCurrentIps(alias, baseIpPart);

            var desiredIps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            desiredIps.Add(baseIpPart + "1");

            foreach (var host in config.Hosts)
            {
                if (host.Enabled) desiredIps.Add(host.IpAddress);
            }

            // Remove IPs that are no longer desired
            foreach (var ip in currentIps)
            {
                if (!desiredIps.Contains(ip))
                {
                    LogBus.Log($"Cleaning up IP {ip}...");
                    RunPowerShell($"Remove-NetIPAddress -InterfaceAlias '{alias}' -IPAddress '{ip}' -Confirm:$false -ErrorAction SilentlyContinue");
                }
            }

            // Add IPs that are missing
            foreach (var ip in desiredIps)
            {
                if (!currentIps.Contains(ip))
                {
                    LogBus.Log($"Sync: Adding IP {ip} to {alias}...");
                    RunPowerShell($"New-NetIPAddress -InterfaceAlias '{alias}' -IPAddress '{ip}' -PrefixLength {prefixLength} -Confirm:$false -ErrorAction SilentlyContinue");
                }
            }
        }
        finally { _syncLock.Release(); }
    }

    private static HashSet<string> GetCurrentIps(string alias, string baseIpPart)
    {
        var ips = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var output = RunPowerShell($"Get-NetIPAddress -InterfaceAlias '{alias}' | Where-Object {{ $_.IPAddress -like '{baseIpPart}*' }} | Select-Object -ExpandProperty IPAddress");
        
        if (!string.IsNullOrWhiteSpace(output))
        {
            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !trimmed.Contains(":")) ips.Add(trimmed);
            }
        }
        return ips;
    }

    private static string ExtractBaseIpPart(string baseNetwork)
    {
        var match = Regex.Match(baseNetwork, @"^(\d+\.\d+\.\d+\.)");
        return match.Success ? match.Groups[1].Value : "198.51.100.";
    }

    private static int ExtractPrefixLength(string baseNetwork)
    {
        var match = Regex.Match(baseNetwork, @"/(\d+)$");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int length)) return length;
        return 24;
    }

    private static string RunPowerShell(string command)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -Command \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(processInfo);
            if (process == null) return string.Empty;

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                LogBus.Log($"PS Error: {error.Trim()}");
            }

            return output;
        }
        catch (Exception ex)
        {
            LogBus.Log($"Execution Error: {ex.Message}");
            return string.Empty;
        }
    }

    public static bool IsRunningAsAdmin()
    {
        using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        var principal = new System.Security.Principal.WindowsPrincipal(identity);
        return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
    }
}



