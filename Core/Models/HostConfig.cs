using System.Collections.Generic;

namespace FakeHostLocalLab.Core.Models;

public class HostConfig
{
    public string Name { get; set; } = "New Host";
    public string IpAddress { get; set; } = "198.51.100.2";
    public bool Enabled { get; set; } = true;
    public List<PortRule> Ports { get; set; } = new List<PortRule>
    {
        new() { Proto = Protocol.TCP, Port = 21, Mode = PortMode.Banner, Response = "220 FTP Server Ready" },
        new() { Proto = Protocol.TCP, Port = 22, Mode = PortMode.Banner, Response = "SSH-2.0-OpenSSH_8.2p1 Ubuntu-4ubuntu0.1" },
        new() { Proto = Protocol.TCP, Port = 80, Mode = PortMode.HttpStatic, Response = "Welcome to LIHT Static Page" },
        new() { Proto = Protocol.TCP, Port = 554, Mode = PortMode.Banner, Response = "RTSP/1.0 200 OK" },
        new() { Proto = Protocol.TCP, Port = 8080, Mode = PortMode.HttpStatic, Response = "8080 Service Ready" },
        new() { Proto = Protocol.TCP, Port = 3389, Mode = PortMode.Banner, Response = "RDP Service Ready" }
    };

    public override string ToString() => $"{Name} ({IpAddress})";
}
