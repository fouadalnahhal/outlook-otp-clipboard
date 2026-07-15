namespace OutlookOtpClipboard;

public sealed class FileLogger
{
    private readonly string directory;

    public FileLogger()
    {
        directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OutlookOtpClipboard", "logs");
        Directory.CreateDirectory(directory);
    }

    public void Write(string message)
    {
        try
        {
            var path = Path.Combine(directory, $"{DateTime.Today:yyyy-MM-dd}.log");
            File.AppendAllText(path, $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
        }
        catch (IOException)
        {
            // Logging must never interrupt OTP monitoring.
        }
    }
}
