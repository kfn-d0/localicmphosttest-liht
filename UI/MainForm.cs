using System;
using System.Linq;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using FakeHostLocalLab.Core.Engine;
using FakeHostLocalLab.Core.Models;
using FakeHostLocalLab.Core.Services;

namespace FakeHostLocalLab.UI;

public partial class MainForm : Form
{
    private AppConfig _config;
    private readonly FakeHostEngine _engine = new();
    private NotifyIcon _trayIcon = null!;

    private System.Windows.Forms.Timer _statusTimer = null!;

    public MainForm()
    {
        InitializeComponent();
        _config = ConfigStore.Load();
        

        SetupTrayIcon();
        LogBus.OnLog += (msg) => this.Invoke(() => AppendLog(msg));
        RefreshHostList();
        SetupStatusTimer();
    }

    private void SetupStatusTimer()
    {
        _statusTimer = new System.Windows.Forms.Timer { Interval = 3000 };
        _statusTimer.Tick += async (s, e) => await UpdateHostStatusAsync();
        _statusTimer.Start();
    }

    private async Task UpdateHostStatusAsync()
    {
        try 
        {
            foreach (DataGridViewRow row in dgvHosts.Rows)
            {
                if (row.Tag is HostConfig host)
                {
                    if (!host.Enabled)
                    {
                        row.Cells[0].Value = "⚫"; 
                        row.Cells[0].Style.ForeColor = System.Drawing.Color.Gray;
                        continue;
                    }

                    try
                    {
                        using var pinger = new Ping();
                        var reply = await pinger.SendPingAsync(host.IpAddress, 200);
                        if (reply.Status == IPStatus.Success)
                        {
                            row.Cells[0].Value = "🟢"; 
                            row.Cells[0].Style.ForeColor = System.Drawing.Color.LimeGreen;
                        }
                        else
                        {
                            row.Cells[0].Value = "🔴"; 
                            row.Cells[0].Style.ForeColor = System.Drawing.Color.Red;
                        }
                    }
                    catch
                    {
                        row.Cells[0].Value = "🔴";
                        row.Cells[0].Style.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
        }
        catch {  }
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new NotifyIcon
        {
            Icon = this.Icon,
            Text = "Local ICMP Host Test - LIHT",
            Visible = true
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Start/Stop", null, (s, e) => btnStartStop_Click(this, EventArgs.Empty));
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Exit", null, (s, e) => {
            _trayIcon.Visible = false;
            _engine.Stop();
            Application.Exit();
        });

        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.DoubleClick += (s, e) => this.Show();
    }

    private void RefreshHostList()
    {
        if (dgvHosts.Columns.Count < 5) return;
        dgvHosts.Rows.Clear();
        foreach (var host in _config.Hosts)
        {
            int index = dgvHosts.Rows.Add("⚪", host.Name, host.IpAddress, host.Enabled, "Copy");
            dgvHosts.Rows[index].Tag = host;
        }
    }

    private void RefreshPortList()
    {
        dgvPorts.Rows.Clear();
        if (dgvHosts.CurrentRow?.Tag is HostConfig selectedHost)
        {
            foreach (var port in selectedHost.Ports)
            {
                dgvPorts.Rows.Add(port.Proto, port.Port, port.Mode, port.Response);
            }
        }
    }

    private void AppendLog(string message)
    {
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        txtLog.SelectionStart = txtLog.Text.Length;
        txtLog.ScrollToCaret();
    }

    private void btnStartStop_Click(object sender, EventArgs e)
    {
        if (_engine.IsRunning)
        {
            _engine.Stop(_config);
            btnStartStop.Text = "Start Engine";
        }
        else
        {
            _engine.Start(_config);
            btnStartStop.Text = "Stop Engine";
        }
    }

    private void dgvHosts_SelectionChanged(object sender, EventArgs e)
    {
        RefreshPortList();
    }

    private void dgvHosts_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (e.ColumnIndex == 3)
        {
            dgvHosts.EndEdit();
            if (dgvHosts.Rows[e.RowIndex].Tag is HostConfig host)
            {
                var val = dgvHosts.Rows[e.RowIndex].Cells[3].Value;
                host.Enabled = val is bool b && b;
                ConfigStore.Save(_config);

                if (_engine.IsRunning)
                {
                    Task.Run(() => NetworkInterfaceManager.SyncIps(_config));
                }
            }
        }
        else if (e.ColumnIndex == 4)
        {
            if (dgvHosts.Rows[e.RowIndex].Tag is HostConfig host)
            {
                try { Clipboard.SetText(host.IpAddress); }
                catch (Exception ex) { LogBus.Log($"Clipboard Error: {ex.Message}"); }
            }
        }
    }

    private void btnCleanup_Click(object sender, EventArgs e)
    {
        var confirm = MessageBox.Show(
            "This will remove the LIHT-Net virtual switch from Hyper-V.\n\nThe adapter 'vEthernet (LIHT-Net)' and all assigned IPs will be permanently deleted.\n\nThe engine will be stopped first.\n\nContinue?",
            "Cleanup Network",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes) return;

        if (_engine.IsRunning)
        {
            _engine.Stop();
            btnStartStop.Text = "Start Engine";
        }

        btnCleanup.Enabled = false;
        Task.Run(() =>
        {
            NetworkInterfaceManager.RemoveLabSwitch();
            this.Invoke(() => btnCleanup.Enabled = true);
        });
    }


    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            _trayIcon.Visible = false;
            _engine.Stop();
        }
        base.OnFormClosing(e);
    }
}
