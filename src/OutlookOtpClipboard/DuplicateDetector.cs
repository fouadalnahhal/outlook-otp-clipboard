namespace OutlookOtpClipboard;

public sealed class DuplicateDetector
{
    private readonly Dictionary<string, DateTimeOffset> recent = new();

    public bool IsDuplicate(string code, string sender, DateTimeOffset now)
    {
        var key = $"{sender}\u001F{code}";
        if (recent.TryGetValue(key, out var previous) && now - previous <= TimeSpan.FromMinutes(5))
        {
            return true;
        }

        recent[key] = now;
        foreach (var staleKey in recent.Where(item => now - item.Value > TimeSpan.FromMinutes(5)).Select(item => item.Key).ToList())
        {
            recent.Remove(staleKey);
        }

        return false;
    }
}
