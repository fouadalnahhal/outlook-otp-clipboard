using System.Text.Json;

namespace OutlookOtpClipboard;

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };
    private readonly string path;

    public SettingsStore()
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OutlookOtpClipboard");
        Directory.CreateDirectory(directory);
        path = Path.Combine(directory, "settings.json");
    }

    public AppSettings Load()
    {
        try
        {
            return File.Exists(path)
                ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), SerializerOptions) ?? new AppSettings()
                : new AppSettings();
        }
        catch (Exception)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings) => File.WriteAllText(path, JsonSerializer.Serialize(settings, SerializerOptions));
}
