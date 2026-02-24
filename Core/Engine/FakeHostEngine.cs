using FakeHostLocalLab.Core.Models;
using FakeHostLocalLab.Core.Services;

namespace FakeHostLocalLab.Core.Engine;

public class FakeHostEngine
{
    private readonly List<TcpPortWorker> _tcpWorkers = new();
    private readonly List<UdpPortWorker> _udpWorkers = new();
    private bool _isRunning;

    public bool IsRunning => _isRunning;

    public void Start(AppConfig config)
    {
        if (_isRunning) Stop();

        LogBus.Log("Starting LIHT Engine...");

        if (!NetworkInterfaceManager.IsRunningAsAdmin())
        {
            LogBus.Log("WARNING: Application not running as Administrator. IP assignment may fail.");
        }

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
            if (!host.Enabled) continue;

            foreach (var rule in host.Ports)
            {
                if (rule.Proto == Protocol.TCP)
                {
                    var worker = new TcpPortWorker(host, rule);
                    worker.Start();
                    _tcpWorkers.Add(worker);
                }
                else
                {
                    var worker = new UdpPortWorker(host, rule);
                    worker.Start();
                    _udpWorkers.Add(worker);
                }
            }
        }

        _isRunning = true;
        LogBus.Log("Engine running.");
    }

    public void Stop(AppConfig? config = null)
    {
        LogBus.Log("Stopping LIHT Engine...");

        foreach (var worker in _tcpWorkers) worker.Stop();
        foreach (var worker in _udpWorkers) worker.Stop();

        _tcpWorkers.Clear();
        _udpWorkers.Clear();
        _isRunning = false;

        if (config != null)
        {
            try { NetworkInterfaceManager.RemoveAllIps(config); }
            catch (Exception ex) { LogBus.Log($"IP Cleanup Error: {ex.Message}"); }
        }

        LogBus.Log("Engine stopped.");
    }
}
