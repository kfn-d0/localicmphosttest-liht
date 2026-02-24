using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeHostLocalLab.Core.Models;
using FakeHostLocalLab.Core.Services;

namespace FakeHostLocalLab.Core.Engine;

public class TcpPortWorker
{
    private readonly HostConfig _host;
    private readonly PortRule _rule;
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;

    public TcpPortWorker(HostConfig host, PortRule rule)
    {
        _host = host;
        _rule = rule;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        var ip = IPAddress.Parse(_host.IpAddress);

        Task.Run(async () => {
            int retries = 5;
            while (retries > 0)
            {
                try
                {
                    _listener = new TcpListener(ip, _rule.Port);
                    _listener.Start();
                    await AcceptLoop(_cts.Token);
                    return;
                }
                catch
                {
                    retries--;
                    if (retries > 0)
                        await Task.Delay(1000);
                }
            }
        });
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
        LogBus.Log($"Stopped TCP {_host.IpAddress}:{_rule.Port}");
    }

    private async Task AcceptLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var client = await _listener!.AcceptTcpClientAsync(token);
                _ = HandleClient(client, token);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                    LogBus.Log($"Error accepting TCP client on {_host.IpAddress}:{_rule.Port}: {ex.Message}");
            }
        }
    }

    private async Task HandleClient(TcpClient client, CancellationToken token)
    {
        using (client)
        {
            try
            {
                if (_rule.DelayMs > 0)
                    await Task.Delay(_rule.DelayMs, token);

                using var stream = client.GetStream();
                
                switch (_rule.Mode)
                {
                    case PortMode.Banner:
                        var banner = Encoding.UTF8.GetBytes(_rule.Response + "\r\n");
                        await stream.WriteAsync(banner, 0, banner.Length, token);
                        break;

                    case PortMode.HttpStatic:
                        var response = "HTTP/1.1 200 OK\r\n" +
                                     "Content-Type: text/plain\r\n" +
                                     "Content-Length: " + _rule.Response.Length + "\r\n" +
                                     "Connection: close\r\n\r\n" +
                                     _rule.Response;
                        var httpData = Encoding.UTF8.GetBytes(response);
                        await stream.WriteAsync(httpData, 0, httpData.Length, token);
                        break;

                    case PortMode.OpenSilent:
                    default:

                        break;
                }
                
                await stream.FlushAsync(token);
            }
            catch (Exception ex)
            {
                 LogBus.Log($"Error handling TCP client on {_host.IpAddress}:{_rule.Port}: {ex.Message}");
            }
        }
    }
}
