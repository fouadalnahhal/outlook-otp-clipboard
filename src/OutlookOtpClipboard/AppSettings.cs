namespace OutlookOtpClipboard;

public sealed class AppSettings
{
    public bool StartWithWindows { get; set; } = true;
    public bool MonitoringPaused { get; set; }
    public string InboxFolder { get; set; } = "Inbox";
    public string RegexPattern { get; set; } = @"\b\d{4,8}\b";
    public int MinimumOtpLength { get; set; } = 4;
    public int MaximumOtpLength { get; set; } = 8;
    public string AllowedSenders { get; set; } = string.Empty;
    public string IgnoredSenders { get; set; } = string.Empty;
    public string SubjectContains { get; set; } = string.Empty;
    public string SubjectDoesNotContain { get; set; } = string.Empty;
    public bool AutoCopy { get; set; } = true;
    public bool ShowToast { get; set; } = true;
    public int HistoryLimit { get; set; } = 20;
}
