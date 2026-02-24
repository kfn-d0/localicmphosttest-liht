using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeHostLocalLab.Core.Models;
using FakeHostLocalLab.Core.Services;

namespace FakeHostLocalLab.Core.Engine;

public class UdpPortWorker
{
    private readonly HostConfig _host;
    private readonly PortRule _rule;
    private UdpClient? _listener;
    private CancellationTokenSource? _cts;

    public UdpPortWorker(HostConfig host, PortRule rule)
    {
        _host = host;
        _rule = rule;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        try
        {
            var ip = IPAddress.Parse(_host.IpAddress);
            var localEndPoint = new IPEndPoint(ip, _rule.Port);
            _listener = new UdpClient(localEndPoint);
            Task.Run(() => ReceiveLoop(_cts.Token));
            LogBus.Log($"Started UDP {_host.IpAddress}:{_rule.Port} ({_rule.Mode})");
        }
        catch (Exception ex)
        {
            LogBus.Log($"Failed to start UDP {_host.IpAddress}:{_rule.Port}: {ex.Message}");
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Close();
        LogBus.Log($"Stopped UDP {_host.IpAddress}:{_rule.Port}");
    }

    private async Task ReceiveLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var result = await _listener!.ReceiveAsync(token);
                
                if (_rule.DelayMs > 0)
                    await Task.Delay(_rule.DelayMs, token);

                if (_rule.Mode == PortMode.UdpEcho)
                {
                    await _listener.SendAsync(result.Buffer, result.Buffer.Length, result.RemoteEndPoint);
                }
                else if (!string.IsNullOrEmpty(_rule.Response))
                {
                    var responseData = Encoding.UTF8.GetBytes(_rule.Response);
                    await _listener.SendAsync(responseData, responseData.Length, result.RemoteEndPoint);
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    LogBus.Log($"Error in UDP listener on {_host.IpAddress}:{_rule.Port}: {ex.Message}");
            }
        }
    }
}



