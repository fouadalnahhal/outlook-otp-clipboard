namespace OutlookOtpClipboard;

public sealed class SettingsForm : Form
{
    private readonly AppSettings settings;
    private readonly CheckBox startup = new() { Text = "Start with Windows", AutoSize = true };
    private readonly TextBox pattern = new() { Width = 320 };
    private readonly NumericUpDown minimum = new() { Minimum = 1, Maximum = 20, Width = 60 };
    private readonly NumericUpDown maximum = new() { Minimum = 1, Maximum = 20, Width = 60 };
    private readonly TextBox allowedSenders = new() { Width = 320 };
    private readonly TextBox ignoredSenders = new() { Width = 320 };
    private readonly TextBox subjectContains = new() { Width = 320 };
    private readonly TextBox subjectDoesNotContain = new() { Width = 320 };
    private readonly CheckBox autoCopy = new() { Text = "Automatically copy OTP", AutoSize = true };
    private readonly CheckBox showToast = new() { Text = "Show notification", AutoSize = true };
    private readonly NumericUpDown historyLimit = new() { Minimum = 1, Maximum = 100, Width = 60 };

    public AppSettings Settings => settings;

    public SettingsForm(AppSettings values, IEnumerable<OtpHistoryItem> history)
    {
        settings = values;
        Text = "Outlook OTP Clipboard Settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(540, 560);

        startup.Checked = values.StartWithWindows;
        pattern.Text = values.RegexPattern;
        minimum.Value = values.MinimumOtpLength;
        maximum.Value = values.MaximumOtpLength;
        allowedSenders.Text = values.AllowedSenders;
        ignoredSenders.Text = values.IgnoredSenders;
        subjectContains.Text = values.SubjectContains;
        subjectDoesNotContain.Text = values.SubjectDoesNotContain;
        autoCopy.Checked = values.AutoCopy;
        showToast.Checked = values.ShowToast;
        historyLimit.Value = values.HistoryLimit;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), ColumnCount = 2, AutoScroll = true };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(layout, "Startup", startup);
        AddRow(layout, "OTP pattern", pattern);
        AddRow(layout, "Minimum length", minimum);
        AddRow(layout, "Maximum length", maximum);
        AddRow(layout, "Allowed senders (;)", allowedSenders);
        AddRow(layout, "Ignored senders (;)", ignoredSenders);
        AddRow(layout, "Subject contains", subjectContains);
        AddRow(layout, "Subject excludes", subjectDoesNotContain);
        AddRow(layout, "Clipboard", autoCopy);
        AddRow(layout, "Notifications", showToast);
        AddRow(layout, "History limit", historyLimit);
        var list = new ListBox { Width = 470, Height = 120 };
        list.Items.AddRange(history.Select(item => $"{item.Time:t} | {item.Sender} | {item.Code}").Cast<object>().ToArray());
        AddRow(layout, "Recent OTPs", list);
        var save = new Button { Text = "Save", DialogResult = DialogResult.OK, AutoSize = true };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        var buttons = new FlowLayoutPanel { AutoSize = true };
        buttons.Controls.Add(save); buttons.Controls.Add(cancel);
        AddRow(layout, string.Empty, buttons);
        AcceptButton = save;
        CancelButton = cancel;
        Controls.Add(layout);
        FormClosing += SaveValues;
    }

    private static void AddRow(TableLayoutPanel layout, string label, Control control)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 12, 8) }, 0, layout.RowCount);
        control.Margin = new Padding(3, 5, 3, 5);
        layout.Controls.Add(control, 1, layout.RowCount++);
    }

    private void SaveValues(object? sender, FormClosingEventArgs e)
    {
        if (DialogResult != DialogResult.OK) return;
        settings.StartWithWindows = startup.Checked;
        settings.RegexPattern = pattern.Text.Trim();
        settings.MinimumOtpLength = (int)minimum.Value;
        settings.MaximumOtpLength = (int)maximum.Value;
        settings.AllowedSenders = allowedSenders.Text.Trim();
        settings.IgnoredSenders = ignoredSenders.Text.Trim();
        settings.SubjectContains = subjectContains.Text.Trim();
        settings.SubjectDoesNotContain = subjectDoesNotContain.Text.Trim();
        settings.AutoCopy = autoCopy.Checked;
        settings.ShowToast = showToast.Checked;
        settings.HistoryLimit = (int)historyLimit.Value;
    }
}
