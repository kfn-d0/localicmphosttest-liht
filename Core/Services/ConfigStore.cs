using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using FakeHostLocalLab.Core.Models;

namespace FakeHostLocalLab.Core.Services;

/// <summary>
/// Handles persistent load/save of AppConfig to %APPDATA%\LIHT\config.json.
/// Falls back to the config.json next to the executable when AppData is unavailable.
/// </summary>
public static class ConfigStore
{
    // ── Paths ────────────────────────────────────────────────────────────────
    private static readonly string AppDataPath =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LIHT",
            "config.json");

    // Fallback: same folder as the .exe (useful during development)
    private static readonly string LocalPath =
        Path.Combine(
            AppContext.BaseDirectory,
            "config.json");

    // ── JSON options (enums as strings for human-readable files) ─────────────
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented        = true,
        PropertyNameCaseInsensitive = true,
        Converters           = { new JsonStringEnumConverter() }
    };

    // ── Cached instance ──────────────────────────────────────────────────────
    private static AppConfig? _instance;

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Load config from AppData (or local fallback). Creates defaults if no file exists.
    /// </summary>
    public static AppConfig Load()
    {
        if (_instance != null) return _instance;

        // Try AppData first, then local directory
        string? foundPath = File.Exists(AppDataPath) ? AppDataPath
                          : File.Exists(LocalPath)   ? LocalPath
                          : null;

        if (foundPath != null)
        {
            try
            {
                var json = File.ReadAllText(foundPath);
                var loaded = JsonSerializer.Deserialize<AppConfig>(json, _jsonOptions);
                if (loaded != null)
                {
                    _instance = loaded;
                    LogBus.Log($"[Config] Loaded from: {foundPath}");
                    return _instance;
                }
            }
            catch (Exception ex)
            {
                LogBus.Log($"[Config] Failed to read config ({foundPath}): {ex.Message}. Using defaults.");
            }
        }
        else
        {
            LogBus.Log("[Config] No config file found. Using defaults.");
        }

        _instance = BuildDefaults();
        Save(_instance);   // persist defaults immediately
        return _instance;
    }

    /// <summary>
    /// Persist the current config to %APPDATA%\LIHT\config.json.
    /// </summary>
    public static void Save(AppConfig config)
    {
        _instance = config;
        try
        {
            var dir = Path.GetDirectoryName(AppDataPath)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(config, _jsonOptions);
            File.WriteAllText(AppDataPath, json);
            LogBus.Log($"[Config] Saved to: {AppDataPath}");
        }
        catch (Exception ex)
        {
            LogBus.Log($"[Config] Save failed: {ex.Message}");
        }
    }

    // ── Default config ───────────────────────────────────────────────────────

    private static AppConfig BuildDefaults()
    {
        var defaultPorts = new List<PortRule>
        {
            new() { Proto = Protocol.TCP, Port = 21,   Mode = PortMode.Banner,     Response = "220 FTP Server Ready" },
            new() { Proto = Protocol.TCP, Port = 22,   Mode = PortMode.Banner,     Response = "SSH-2.0-OpenSSH_8.2p1 Ubuntu-4ubuntu0.1" },
            new() { Proto = Protocol.TCP, Port = 80,   Mode = PortMode.HttpStatic, Response = "Welcome to LIHT Static Page" },
            new() { Proto = Protocol.TCP, Port = 554,  Mode = PortMode.Banner,     Response = "RTSP/1.0 200 OK" },
            new() { Proto = Protocol.TCP, Port = 8080, Mode = PortMode.HttpStatic, Response = "8080 Service Ready" },
            new() { Proto = Protocol.TCP, Port = 3389, Mode = PortMode.Banner,     Response = "RDP Service Ready" }
        };

        return new AppConfig
        {
            InterfaceAlias = "AUTO",
            BaseNetwork    = "198.51.100.0/24",
            Hosts = new List<HostConfig>
            {
                new() { Name = "Gateway-001", IpAddress = "198.51.100.1", Enabled = true, Ports = Clone(defaultPorts) },
                new() { Name = "Host-002",    IpAddress = "198.51.100.2", Enabled = true, Ports = Clone(defaultPorts) },
                new() { Name = "Host-003",    IpAddress = "198.51.100.3", Enabled = true, Ports = Clone(defaultPorts) },
                new() { Name = "Host-004",    IpAddress = "198.51.100.4", Enabled = true, Ports = Clone(defaultPorts) },
                new() { Name = "Host-005",    IpAddress = "198.51.100.5", Enabled = true, Ports = Clone(defaultPorts) },
                new() { Name = "Host-006",    IpAddress = "198.51.100.6", Enabled = true, Ports = Clone(defaultPorts) },
            }
        };
    }

    private static List<PortRule> Clone(List<PortRule> source)
    {
        var list = new List<PortRule>();
        foreach (var r in source)
            list.Add(new PortRule { Proto = r.Proto, Port = r.Port, Mode = r.Mode, DelayMs = r.DelayMs, Response = r.Response });
        return list;
    }
}
