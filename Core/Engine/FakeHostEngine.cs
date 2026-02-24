using FakeHostLocalLab.Core.Models;
using FakeHostLocalLab.Core.Services;

namespace FakeHostLocalLab.Core.Engine;

public class FakeHostEngine
{
    // Maps host IpAddress → its active TCP/UDP workers so we can stop per-host.
    private readonly Dictionary<string, List<TcpPortWorker>> _tcpByHost = new();
    private readonly Dictionary<string, List<UdpPortWorker>> _udpByHost = new();
    private bool _isRunning;

    public bool IsRunning => _isRunning;

    // ── Start/Stop all hosts ─────────────────────────────────────────────────

    public void Start(AppConfig config)
    {
        if (_isRunning) Stop(config);

        LogBus.Log("Starting LIHT Engine...");

        if (!NetworkInterfaceManager.IsRunningAsAdmin())
            LogBus.Log("WARNING: Not running as Administrator. IP assignment may fail.");

        try
        {
            NetworkInterfaceManager.SyncIps(config);
            System.Threading.Thread.Sleep(2000);
        }
        catch (Exception ex)
        {
            LogBus.Log($"Network Error: {ex.Message}");
        }

        foreach (var host in config.Hosts)
        {
            if (host.Enabled)
                StartHostListeners(host);
        }

        _isRunning = true;
        LogBus.Log("Engine running.");
    }

    public void Stop(AppConfig? config = null)
    {
        LogBus.Log("Stopping LIHT Engine...");

        StopAllListeners();
        _isRunning = false;

        if (config != null)
        {
            try { NetworkInterfaceManager.RemoveAllIps(config); }
            catch (Exception ex) { LogBus.Log($"IP Cleanup Error: {ex.Message}"); }
        }

        LogBus.Log("Engine stopped.");
    }

    // ── Per-host listener management ─────────────────────────────────────────

    /// <summary>
    /// Starts TCP/UDP listeners for a single host. Safe to call at runtime.
    /// </summary>
    public void StartHostListeners(HostConfig host)
    {
        // Ensure old listeners are cleaned up first (idempotent).
        StopHostListeners(host);

        var tcpList = new List<TcpPortWorker>();
        var udpList = new List<UdpPortWorker>();

        foreach (var rule in host.Ports)
        {
            if (rule.Proto == Protocol.TCP)
            {
                var w = new TcpPortWorker(host, rule);
                w.Start();
                tcpList.Add(w);
            }
            else
            {
                var w = new UdpPortWorker(host, rule);
                w.Start();
                udpList.Add(w);
            }
        }

        _tcpByHost[host.IpAddress] = tcpList;
        _udpByHost[host.IpAddress] = udpList;

        LogBus.Log($"[Engine] Listeners started for {host.Name} ({host.IpAddress}).");
    }

    /// <summary>
    /// Stops all TCP/UDP listeners for a single host. Safe to call at runtime.
    /// </summary>
    public void StopHostListeners(HostConfig host)
    {
        if (_tcpByHost.TryGetValue(host.IpAddress, out var tcpList))
        {
            foreach (var w in tcpList) w.Stop();
            _tcpByHost.Remove(host.IpAddress);
        }

        if (_udpByHost.TryGetValue(host.IpAddress, out var udpList))
        {
            foreach (var w in udpList) w.Stop();
            _udpByHost.Remove(host.IpAddress);
        }

        LogBus.Log($"[Engine] Listeners stopped for {host.Name} ({host.IpAddress}).");
    }

    // ── Internal helpers ─────────────────────────────────────────────────────

    private void StopAllListeners()
    {
        foreach (var list in _tcpByHost.Values)
            foreach (var w in list) w.Stop();
        foreach (var list in _udpByHost.Values)
            foreach (var w in list) w.Stop();

        _tcpByHost.Clear();
        _udpByHost.Clear();
    }
}
