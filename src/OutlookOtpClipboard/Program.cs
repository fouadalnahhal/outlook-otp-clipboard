namespace OutlookOtpClipboard;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var application = new TrayApplicationContext();
        Application.Run(application);
    }
}
