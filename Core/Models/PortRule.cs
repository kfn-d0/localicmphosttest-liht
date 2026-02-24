namespace FakeHostLocalLab.Core.Models;

public enum PortMode
{
    Banner,
    HttpStatic,
    OpenSilent,
    UdpEcho
}

public enum Protocol
{
    TCP,
    UDP
}

public class PortRule
{
    public Protocol Proto { get; set; } = Protocol.TCP;
    public int Port { get; set; }
    public PortMode Mode { get; set; } = PortMode.Banner;
    public int DelayMs { get; set; } = 0;
    public string Response { get; set; } = string.Empty;

    public override string ToString() => $"{Proto} {Port} ({Mode})";
}
