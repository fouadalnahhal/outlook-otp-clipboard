using System.Text.RegularExpressions;

namespace OutlookOtpClipboard;

public sealed class OtpExtractor
{
    public string? Extract(string subject, string body, AppSettings settings)
    {
        try
        {
            var expression = new Regex(settings.RegexPattern, RegexOptions.CultureInvariant);
            foreach (Match match in expression.Matches($"{subject}\n{body}"))
            {
                if (match.Value.Length >= settings.MinimumOtpLength && match.Value.Length <= settings.MaximumOtpLength)
                {
                    return match.Value;
                }
            }
        }
        catch (ArgumentException)
        {
            // Invalid patterns are ignored until the user corrects them in Settings.
        }

        return null;
    }
}
