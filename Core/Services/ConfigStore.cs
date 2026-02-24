using System.Collections.Generic;
using FakeHostLocalLab.Core.Models;

namespace FakeHostLocalLab.Core.Services;

public static class ConfigStore
{
    private static AppConfig? _instance;

    public static AppConfig Load()
    {
        if (_instance != null) return _instance;

        var defaultPorts = new List<PortRule>
        {
            new() { Proto = Protocol.TCP, Port = 21,   Mode = PortMode.Banner,     Response = "220 FTP Server Ready" },
            new() { Proto = Protocol.TCP, Port = 22,   Mode = PortMode.Banner,     Response = "SSH-2.0-OpenSSH_8.2p1 Ubuntu-4ubuntu0.1" },
            new() { Proto = Protocol.TCP, Port = 80,   Mode = PortMode.HttpStatic, Response = "Welcome to LIHT Static Page" },
            new() { Proto = Protocol.TCP, Port = 554,  Mode = PortMode.Banner,     Response = "RTSP/1.0 200 OK" },
            new() { Proto = Protocol.TCP, Port = 8080, Mode = PortMode.HttpStatic, Response = "8080 Service Ready" },
            new() { Proto = Protocol.TCP, Port = 3389, Mode = PortMode.Banner,     Response = "RDP Service Ready" }
        };

        _instance = new AppConfig
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

        return _instance;
    }

    public static void Save(AppConfig config)
    {
        _instance = config;
    }

    private static List<PortRule> Clone(List<PortRule> source)
    {
        var list = new List<PortRule>();
        foreach (var r in source)
            list.Add(new PortRule { Proto = r.Proto, Port = r.Port, Mode = r.Mode, DelayMs = r.DelayMs, Response = r.Response });
        return list;
    }
}
