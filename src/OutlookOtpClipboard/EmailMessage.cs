namespace OutlookOtpClipboard;

public sealed record EmailMessage(string Sender, string Subject, string Body);
public sealed record OtpHistoryItem(DateTimeOffset Time, string Sender, string Subject, string Code);
