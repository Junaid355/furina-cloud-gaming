// d:\cloud\FurinaHost\UI\HostForm.cs
#nullable disable
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using FurinaHost.Models;
using FurinaHost.Services;

namespace FurinaHost.UI;

public class HostForm : Form
{
    private readonly System.Windows.Forms.Timer _updateTimer;
    private Label _lblPin;
    private Label _lblStatus;
    private Label _lblFps;
    private Label _lblBandwidth;
    private Label _lblUrl;
    private Label _lblCpu;
    private Label _lblGpu;
    private Label _lblRam;
    private FlowLayoutPanel _pnlGames;
    private ListBox _lstClients;
    private NotifyIcon _trayIcon;
    private ContextMenuStrip _trayMenu;

    public HostForm()
    {
        InitializeComponent();

        _updateTimer = new System.Windows.Forms.Timer
        {
            Interval = 1000 // 1 sec refresh
        };
        _updateTimer.Tick += (s, e) => UpdateTelemetry();
        _updateTimer.Start();
    }

    private void InitializeComponent()
    {
        this.Text = "Furina Cloud Gaming — Host Control Center";
        this.Size = new Size(880, 680);
        this.MinimumSize = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(8, 12, 22);
        this.ForeColor = Color.FromArgb(226, 232, 240);
        this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

        // Header Panel
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.FromArgb(14, 21, 38),
            Padding = new Padding(20, 10, 20, 10)
        };

        var lblBrand = new Label
        {
            Text = "🌊 FURINA CLOUD GAMING",
            Font = new Font("Segoe UI", 15f, FontStyle.Bold),
            ForeColor = Color.FromArgb(77, 232, 224),
            AutoSize = true,
            Location = new Point(20, 12)
        };

        var lblSubtitle = new Label
        {
            Text = "High-Performance Windows Game Streaming Host for Mobile Devices",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(22, 40)
        };

        var btnEmergency = new Button
        {
            Text = "⚠️ Emergency Stop (Release All Keys)",
            Size = new Size(240, 36),
            Location = new Point(600, 16),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = Color.FromArgb(239, 68, 68),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnEmergency.FlatAppearance.BorderSize = 0;
        btnEmergency.Click += (s, e) =>
        {
            InputInjector.ReleaseAllInputs();
            MessageBox.Show("All physical keys and mouse buttons released immediately.", "Safety Triggered", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        pnlHeader.Controls.AddRange(new Control[] { lblBrand, lblSubtitle, btnEmergency });
        this.Controls.Add(pnlHeader);

        // Tabs
        var tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Padding = new Point(14, 8)
        };

        // Tab 1: Status & Pairing
        var tabStatus = new TabPage("⚡ Status & Pairing") { BackColor = Color.FromArgb(8, 12, 22) };
        BuildStatusTab(tabStatus);

        // Tab 2: Game Library
        var tabGames = new TabPage("🎮 Game Library") { BackColor = Color.FromArgb(8, 12, 22) };
        BuildGamesTab(tabGames);

        // Tab 3: Connected Clients & Tools
        var tabClients = new TabPage("📱 Connected Devices") { BackColor = Color.FromArgb(8, 12, 22) };
        BuildClientsTab(tabClients);

        tabControl.TabPages.AddRange(new TabPage[] { tabStatus, tabGames, tabClients });
        this.Controls.Add(tabControl);

        // System Tray Integration
        SetupSystemTray();
    }

    private void BuildStatusTab(TabPage tab)
    {
        var root = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };

        // Card 1: Pairing PIN Box
        var pnlPinCard = new Panel
        {
            Size = new Size(380, 200),
            Location = new Point(20, 20),
            BackColor = Color.FromArgb(14, 21, 38),
            Padding = new Padding(16)
        };
        pnlPinCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlPinCard.ClientRectangle, Color.FromArgb(77, 232, 224), ButtonBorderStyle.Solid);

        var lblPinTitle = new Label
        {
            Text = "🔑 MOBILE PAIRING PIN",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.FromArgb(77, 232, 224),
            Location = new Point(16, 16),
            AutoSize = true
        };

        _lblPin = new Label
        {
            Text = PairingManager.CurrentPin,
            Font = new Font("Consolas", 36f, FontStyle.Bold),
            ForeColor = Color.FromArgb(246, 216, 96),
            Location = new Point(16, 50),
            AutoSize = true
        };

        var lblPinHint = new Label
        {
            Text = "Enter this 4-digit PIN on your iPhone/Android to connect.",
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(148, 163, 184),
            Location = new Point(18, 115),
            Size = new Size(340, 36)
        };

        var btnRegenPin = new Button
        {
            Text = "🔄 Regenerate PIN",
            Size = new Size(160, 32),
            Location = new Point(18, 152),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f)
        };
        btnRegenPin.FlatAppearance.BorderColor = Color.FromArgb(77, 232, 224);
        btnRegenPin.Click += (s, e) =>
        {
            _lblPin.Text = PairingManager.RegeneratePin();
        };

        pnlPinCard.Controls.AddRange(new Control[] { lblPinTitle, _lblPin, lblPinHint, btnRegenPin });

        // Card 2: Server Address & Telemetry
        var pnlServerCard = new Panel
        {
            Size = new Size(400, 200),
            Location = new Point(420, 20),
            BackColor = Color.FromArgb(14, 21, 38),
            Padding = new Padding(16)
        };
        pnlServerCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlServerCard.ClientRectangle, Color.FromArgb(51, 65, 85), ButtonBorderStyle.Solid);

        var lblServerTitle = new Label
        {
            Text = "🌐 SERVER ACCESS & TELEMETRY",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.FromArgb(92, 160, 242),
            Location = new Point(16, 16),
            AutoSize = true
        };

        var hw = HardwareDetector.GetHardwareInfo();
        var localIp = hw.LocalIpAddresses.FirstOrDefault() ?? "127.0.0.1";
        var serverUrl = $"http://{localIp}:8080";

        _lblUrl = new Label
        {
            Text = $"Mobile URL: {serverUrl}",
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(16, 46),
            AutoSize = true
        };

        _lblStatus = new Label
        {
            Text = "● Stream Engine: Active (Listening on 0.0.0.0:8080)",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(16, 185, 129),
            Location = new Point(16, 75),
            AutoSize = true
        };

        _lblFps = new Label
        {
            Text = "Stream FPS: 60 FPS",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(226, 232, 240),
            Location = new Point(16, 100),
            AutoSize = true
        };

        _lblBandwidth = new Label
        {
            Text = "Bandwidth: ~3.2 Mbps",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(226, 232, 240),
            Location = new Point(16, 125),
            AutoSize = true
        };

        var btnOpenBrowser = new Button
        {
            Text = "🌐 Open Web Dashboard",
            Size = new Size(170, 32),
            Location = new Point(16, 152),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnOpenBrowser.FlatAppearance.BorderColor = Color.FromArgb(92, 160, 242);
        btnOpenBrowser.Click += (s, e) =>
        {
            Process.Start(new ProcessStartInfo(serverUrl) { UseShellExecute = true });
        };

        var btnUnblockFw = new Button
        {
            Text = "🛡️ Unblock Firewall",
            Size = new Size(160, 32),
            Location = new Point(195, 152),
            BackColor = Color.FromArgb(30, 41, 59),
            ForeColor = Color.FromArgb(246, 216, 96),
            FlatStyle = FlatStyle.Flat
        };
        btnUnblockFw.FlatAppearance.BorderColor = Color.FromArgb(246, 216, 96);
        btnUnblockFw.Click += (s, e) =>
        {
            Process.Start(new ProcessStartInfo("d:\\cloud\\allow_firewall.bat") { UseShellExecute = true });
        };

        pnlServerCard.Controls.AddRange(new Control[] { lblServerTitle, _lblUrl, _lblStatus, _lblFps, _lblBandwidth, btnOpenBrowser, btnUnblockFw });

        // Card 3: Hardware Specifications
        var pnlHwCard = new Panel
        {
            Size = new Size(800, 160),
            Location = new Point(20, 240),
            BackColor = Color.FromArgb(14, 21, 38),
            Padding = new Padding(16)
        };
        pnlHwCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlHwCard.ClientRectangle, Color.FromArgb(51, 65, 85), ButtonBorderStyle.Solid);

        var lblHwTitle = new Label
        {
            Text = "💻 HOST HARDWARE MONITOR",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.FromArgb(168, 85, 247),
            Location = new Point(16, 16),
            AutoSize = true
        };

        _lblCpu = new Label
        {
            Text = $"Processor: {hw.CpuName} ({hw.CpuCores} Cores / 12 Threads)",
            Font = new Font("Segoe UI", 9.5f),
            Location = new Point(16, 48),
            AutoSize = true
        };

        var gpuName = hw.Gpus.Count > 0 ? hw.Gpus[0].Name : "GPU";
        _lblGpu = new Label
        {
            Text = $"Graphics: {gpuName} (Primary Display: {hw.Displays.FirstOrDefault()?.Width ?? 1920}x{hw.Displays.FirstOrDefault()?.Height ?? 1080} @ {hw.Displays.FirstOrDefault()?.RefreshRate ?? 60}Hz)",
            Font = new Font("Segoe UI", 9.5f),
            Location = new Point(16, 76),
            AutoSize = true
        };

        _lblRam = new Label
        {
            Text = $"Memory: {hw.TotalRamGb} GB RAM (Free: {hw.FreeRamGb} GB)",
            Font = new Font("Segoe UI", 9.5f),
            Location = new Point(16, 104),
            AutoSize = true
        };

        pnlHwCard.Controls.AddRange(new Control[] { lblHwTitle, _lblCpu, _lblGpu, _lblRam });

        root.Controls.AddRange(new Control[] { pnlPinCard, pnlServerCard, pnlHwCard });
        tab.Controls.Add(root);
    }

    private void BuildGamesTab(TabPage tab)
    {
        var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

        var lblGamesHeader = new Label
        {
            Text = "Installed Games & One-Click Launchers",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Top,
            Height = 35
        };

        _pnlGames = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 10, 0, 0)
        };

        PopulateGamesList();

        root.Controls.AddRange(new Control[] { _pnlGames, lblGamesHeader });
        tab.Controls.Add(root);
    }

    private void PopulateGamesList()
    {
        _pnlGames.Controls.Clear();
        var games = GameManager.GetAllGames();

        foreach (var game in games)
        {
            var pnlGame = new Panel
            {
                Size = new Size(250, 140),
                BackColor = Color.FromArgb(14, 21, 38),
                Margin = new Padding(10)
            };
            pnlGame.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlGame.ClientRectangle, Color.FromArgb(51, 65, 85), ButtonBorderStyle.Solid);

            var lblName = new Label
            {
                Text = game.Name,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 12),
                Size = new Size(220, 22)
            };

            var lblCat = new Label
            {
                Text = game.Category,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(12, 36),
                Size = new Size(220, 18)
            };

            var lblStatus = new Label
            {
                Text = game.IsInstalled ? "✓ Detected on PC" : "○ Configured",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = game.IsInstalled ? Color.FromArgb(16, 185, 129) : Color.FromArgb(148, 163, 184),
                Location = new Point(12, 58),
                Size = new Size(220, 18)
            };

            var btnLaunch = new Button
            {
                Text = "▶ Launch",
                Size = new Size(100, 30),
                Location = new Point(12, 90),
                BackColor = Color.FromArgb(77, 232, 224),
                ForeColor = Color.FromArgb(8, 12, 22),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnLaunch.Click += (s, e) =>
            {
                var ok = GameManager.LaunchGame(game.Id, out var msg);
                MessageBox.Show(ok ? $"Launched {game.Name} on host PC." : $"Could not launch {game.Name}: {msg}", "Game Launcher", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            };

            var btnStop = new Button
            {
                Text = "⏹ Stop",
                Size = new Size(100, 30),
                Location = new Point(120, 90),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.FromArgb(239, 68, 68),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnStop.FlatAppearance.BorderColor = Color.FromArgb(239, 68, 68);
            btnStop.Click += (s, e) =>
            {
                GameManager.StopGame(game.Id, out var msg);
                MessageBox.Show($"Terminated {game.Name} process: {msg}", "Game Launcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            pnlGame.Controls.AddRange(new Control[] { lblName, lblCat, lblStatus, btnLaunch, btnStop });
            _pnlGames.Controls.Add(pnlGame);
        }
    }

    private void BuildClientsTab(TabPage tab)
    {
        var root = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

        var lblClientsHeader = new Label
        {
            Text = "Active Mobile Connections",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Top,
            Height = 35
        };

        _lstClients = new ListBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(14, 21, 38),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10f),
            BorderStyle = BorderStyle.FixedSingle
        };

        var pnlActions = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(0, 10, 0, 0) };
        var btnDisconnectAll = new Button
        {
            Text = "Disconnect All Clients",
            Size = new Size(180, 34),
            BackColor = Color.FromArgb(239, 68, 68),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnDisconnectAll.Click += (s, e) =>
        {
            var sessions = PairingManager.GetActiveSessions();
            foreach (var sess in sessions)
            {
                PairingManager.DisconnectClient(sess.ClientId);
            }
            UpdateTelemetry();
            MessageBox.Show("Disconnected all active mobile devices.", "Clients Revoked", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        pnlActions.Controls.Add(btnDisconnectAll);

        root.Controls.AddRange(new Control[] { _lstClients, pnlActions, lblClientsHeader });
        tab.Controls.Add(root);
    }

    private void SetupSystemTray()
    {
        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add("Open Host Control Center", null, (s, e) => ShowFromTray());
        _trayMenu.Items.Add("Emergency Input Release", null, (s, e) => InputInjector.ReleaseAllInputs());
        _trayMenu.Items.Add("-");
        _trayMenu.Items.Add("Exit Furina Host", null, (s, e) => ExitApplication());

        _trayIcon = new NotifyIcon
        {
            Text = "Furina Cloud Gaming Host",
            Icon = SystemIcons.Application,
            ContextMenuStrip = _trayMenu,
            Visible = true
        };
        _trayIcon.DoubleClick += (s, e) => ShowFromTray();

        this.FormClosing += (s, e) =>
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                _trayIcon.ShowBalloonTip(2000, "Furina Cloud Gaming", "Host running in system tray. Streaming remains active.", ToolTipIcon.Info);
            }
        };
    }

    private void ShowFromTray()
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.BringToFront();
    }

    private void ExitApplication()
    {
        _updateTimer.Stop();
        _trayIcon.Visible = false;
        Application.Exit();
        Environment.Exit(0);
    }

    private void UpdateTelemetry()
    {
        if (this.IsDisposed) return;

        try
        {
            _lblPin.Text = PairingManager.CurrentPin;
            var fps = Math.Round(ScreenCapture.Instance.CurrentFps, 1);
            _lblFps.Text = $"Stream FPS: {fps} FPS (Capture: {Math.Round(ScreenCapture.Instance.AverageEncodeMs, 1)}ms)";

            var mbps = Math.Round((ScreenCapture.Instance.TotalBytesSent / 1024.0 / 1024.0), 2);
            _lblBandwidth.Text = $"Total Data Broadcast: {mbps} MB";

            var sessions = PairingManager.GetActiveSessions();
            _lstClients.Items.Clear();
            if (sessions.Count == 0)
            {
                _lstClients.Items.Add("No mobile devices currently connected. Open http://192.168.1.197:8080 on your phone.");
            }
            else
            {
                foreach (var s in sessions)
                {
                    _lstClients.Items.Add($"{s.DeviceName} ({s.RemoteIp}) - Connected at {s.ConnectedAt.ToLocalTime():T}");
                }
            }
        }
        catch {}
    }
}
