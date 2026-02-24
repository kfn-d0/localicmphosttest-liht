using System.Collections.Generic;

namespace FakeHostLocalLab.Core.Models;

public class AppConfig
{
    public string InterfaceAlias { get; set; } = "AUTO";
    public string BaseNetwork { get; set; } = "198.51.100.0/24";
    public List<HostConfig> Hosts { get; set; } = new();
}
