using System.Windows;

namespace ClickOw.Settings;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;

    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        DataContext = _settings;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        // Apply startup preference and persist on close.
        StartupManager.Apply(_settings.RunAtStartup);
        SettingsStore.Save(_settings);
        base.OnClosed(e);
    }
}
