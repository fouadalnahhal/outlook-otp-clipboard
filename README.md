# Outlook OTP Clipboard Assistant

A lightweight Windows tray app that listens to the installed classic Outlook desktop client, finds OTPs in new email, copies them to the clipboard, and shows a Windows notification. All data remains on the computer.

## Requirements

- Windows 10/11
- .NET 8 SDK to build, or the self-contained published executable to run
- Classic Microsoft Outlook desktop installed and running

> New Outlook does not expose the legacy Outlook COM event API used by this app.

## Easy installation

For most users, download the latest ZIP from the [GitHub Releases page](https://github.com/fouadalnahhal/outlook-otp-clipboard/releases), extract it to a permanent folder, and run `OutlookOtpClipboard.exe`.

No .NET SDK, PowerShell commands, or source-code build is required. The ZIP includes an `INSTALL.txt` guide. Keep classic Outlook running while using the app.

## Run locally

```powershell
dotnet run --project src/OutlookOtpClipboard
```

The tray menu opens Settings, pauses monitoring, copies the latest OTP, or exits. While Outlook is unavailable the app stays in the tray and retries every 10 seconds.

## Build and publish

```powershell
.\build.ps1
.\build.ps1 -Publish
```

The single-file executable is written to `publish\OutlookOtpClipboard.exe`.

## Installer

Install [Inno Setup](https://jrsoftware.org/isinfo.php), run `build.ps1 -Publish`, then compile `installer\OutlookOtpClipboard.iss` with the Inno Setup Compiler. The installer is per-user, makes a Start Menu shortcut, optionally makes a desktop shortcut, and registers launch at sign-in.

## Settings

- OTP matching defaults to `\b\d{4,8}\b` and accepts a custom .NET regular expression.
- Sender allow/ignore lists use semicolon-separated partial matches.
- Duplicate `(sender, OTP)` pairs are ignored for five minutes.
- Settings and daily logs live in `%LOCALAPPDATA%\OutlookOtpClipboard`.
- OTP history stays in memory and is cleared when the application exits.
