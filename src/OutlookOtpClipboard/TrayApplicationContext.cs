namespace OutlookOtpClipboard;

public sealed class TrayApplicationContext : ApplicationContext
{
    private readonly SettingsStore settingsStore = new();
    private readonly FileLogger logger = new();
    private readonly OtpExtractor extractor = new();
    private readonly DuplicateDetector duplicates = new();
    private readonly List<OtpHistoryItem> history = [];
    private readonly NotifyIcon trayIcon;
    private readonly Control clipboardDispatcher = new();
    private readonly ToolStripMenuItem pauseItem;
    private readonly ToolStripMenuItem resumeItem;
    private readonly System.Windows.Forms.Timer reconnectTimer;
    private readonly OutlookListener outlook;
    private AppSettings settings;
    private string? lastOtp;

    public TrayApplicationContext()
    {
        settings = settingsStore.Load();
        trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Information,
            Text = "Outlook OTP Clipboard Assistant",
            Visible = true
        };
        pauseItem = new ToolStripMenuItem("Pause Monitoring", null, (_, _) => SetPaused(true));
        resumeItem = new ToolStripMenuItem("Resume Monitoring", null, (_, _) => SetPaused(false));
        trayIcon.ContextMenuStrip = new ContextMenuStrip();
        trayIcon.ContextMenuStrip.Items.Add("Open Settings", null, (_, _) => OpenSettings());
        trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        trayIcon.ContextMenuStrip.Items.Add(pauseItem);
        trayIcon.ContextMenuStrip.Items.Add(resumeItem);
        trayIcon.ContextMenuStrip.Items.Add("Copy Last OTP", null, (_, _) => CopyLastOtp());
        trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        trayIcon.ContextMenuStrip.Items.Add("Exit", null, (_, _) => ExitApplication());
        trayIcon.DoubleClick += (_, _) => OpenSettings();
        clipboardDispatcher.CreateControl();

        outlook = new OutlookListener(logger);
        outlook.MessageReceived += OnMessageReceived;
        reconnectTimer = new System.Windows.Forms.Timer { Interval = 10_000 };
        reconnectTimer.Tick += (_, _) => ConnectOutlook();
        UpdateMenu();
        logger.Write("Application started.");
        ConnectOutlook();
        reconnectTimer.Start();
    }

    private void ConnectOutlook()
    {
        if (outlook.Connect())
        {
            reconnectTimer.Stop();
            trayIcon.Text = "Outlook OTP Clipboard Assistant";
        }
        else
        {
            trayIcon.Text = "Outlook OTP Clipboard Assistant - Waiting for Outlook";
            reconnectTimer.Start();
        }
    }

    private void OnMessageReceived(object? sender, EmailMessage message)
    {
        if (clipboardDispatcher.InvokeRequired)
        {
            clipboardDispatcher.BeginInvoke(() => ProcessMessage(message));
            return;
        }

        ProcessMessage(message);
    }

    private void ProcessMessage(EmailMessage message)
    {
        if (settings.MonitoringPaused)
        {
            logger.Write("Ignored email because monitoring is paused.");
            return;
        }

        if (!MatchesFilters(message))
        {
            logger.Write($"Ignored email from {message.Sender} because it did not match the configured filters.");
            return;
        }

        var code = extractor.Extract(message.Subject, message.Body, settings);
        if (code is null)
        {
            logger.Write($"No matching OTP found in email from {message.Sender}.");
            return;
        }

        if (duplicates.IsDuplicate(code, message.Sender, DateTimeOffset.Now))
        {
            logger.Write($"Ignored duplicate OTP from {message.Sender}: {code}");
            return;
        }

        lastOtp = code;
        history.Insert(0, new OtpHistoryItem(DateTimeOffset.Now, message.Sender, message.Subject, code));
        if (history.Count > settings.HistoryLimit) history.RemoveRange(settings.HistoryLimit, history.Count - settings.HistoryLimit);
        var copied = !settings.AutoCopy;
        if (settings.AutoCopy)
        {
            try
            {
                Clipboard.SetText(code);
                copied = true;
            }
            catch (Exception exception)
            {
                logger.Write($"Clipboard error: {exception.Message}");
            }
        }
        if (copied)
        {
            logger.Write($"Copied OTP from {message.Sender}: {code}");
            ShowNotification(code);
        }
    }

    private bool MatchesFilters(EmailMessage message)
    {
        static bool ContainsAny(string value, string filters) => filters.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Any(filter => value.Contains(filter, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(settings.AllowedSenders) && !ContainsAny(message.Sender, settings.AllowedSenders)) return false;
        if (!string.IsNullOrWhiteSpace(settings.IgnoredSenders) && ContainsAny(message.Sender, settings.IgnoredSenders)) return false;
        if (!string.IsNullOrWhiteSpace(settings.SubjectContains) && !message.Subject.Contains(settings.SubjectContains, StringComparison.OrdinalIgnoreCase)) return false;
        return string.IsNullOrWhiteSpace(settings.SubjectDoesNotContain) || !message.Subject.Contains(settings.SubjectDoesNotContain, StringComparison.OrdinalIgnoreCase);
    }

    private void ShowNotification(string code)
    {
        if (!settings.ShowToast) return;
        trayIcon.ShowBalloonTip(4_000, "OTP Copied", code, ToolTipIcon.Info);
    }

    private void CopyLastOtp()
    {
        if (lastOtp is not null) Clipboard.SetText(lastOtp);
    }

    private void SetPaused(bool paused)
    {
        settings.MonitoringPaused = paused;
        settingsStore.Save(settings);
        UpdateMenu();
    }

    private void UpdateMenu()
    {
        pauseItem.Enabled = !settings.MonitoringPaused;
        resumeItem.Enabled = settings.MonitoringPaused;
    }

    private void OpenSettings()
    {
        using var dialog = new SettingsForm(settings, history);
        if (dialog.ShowDialog() != DialogResult.OK) return;
        settings = dialog.Settings;
        settingsStore.Save(settings);
        new StartupService().SetEnabled(settings.StartWithWindows);
        UpdateMenu();
    }

    private void ExitApplication()
    {
        reconnectTimer.Stop();
        logger.Write("Application stopped.");
        trayIcon.Visible = false;
        outlook.Dispose();
        ExitThread();
    }
}
