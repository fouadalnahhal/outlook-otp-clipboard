using System.Runtime.InteropServices;

namespace OutlookOtpClipboard;

public sealed class OutlookListener : IDisposable
{
    private dynamic? application;
    private Action<string>? newMailHandler;
    private readonly FileLogger logger;

    public OutlookListener(FileLogger logger) => this.logger = logger;

    public event EventHandler<EmailMessage>? MessageReceived;
    public bool IsConnected => application is not null;

    public bool Connect()
    {
        if (application is not null) return true;
        try
        {
            application = GetActiveObject("Outlook.Application");
            newMailHandler = OnNewMail;
            application.NewMailEx += newMailHandler;
            logger.Write("Connected to Outlook.");
            return true;
        }
        catch (COMException)
        {
            return false;
        }
        catch (Exception exception)
        {
            logger.Write($"Outlook connection error: {exception.Message}");
            return false;
        }
    }

    private void OnNewMail(string entryIdCollection)
    {
        if (application is null) return;
        logger.Write("Outlook NewMailEx event received.");
        foreach (var entryId in entryIdCollection.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                dynamic message = application.Session.GetItemFromID(entryId);
                MessageReceived?.Invoke(this, new EmailMessage(message.SenderEmailAddress ?? string.Empty, message.Subject ?? string.Empty, message.Body ?? string.Empty));
                Marshal.ReleaseComObject(message);
            }
            catch (Exception exception)
            {
                logger.Write($"Outlook message error: {exception.Message}");
            }
        }
    }

    public void Dispose()
    {
        if (application is not null && newMailHandler is not null) application.NewMailEx -= newMailHandler;
        if (application is not null) Marshal.ReleaseComObject(application);
    }

    private static object GetActiveObject(string programmaticIdentifier)
    {
        var type = Type.GetTypeFromProgID(programmaticIdentifier) ?? throw new COMException($"COM class not registered: {programmaticIdentifier}");
        var classId = type.GUID;
        Marshal.ThrowExceptionForHR(GetActiveObject(ref classId, IntPtr.Zero, out var instance));
        return instance;
    }

    [DllImport("oleaut32.dll")]
    private static extern int GetActiveObject(ref Guid classId, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out object instance);
}
