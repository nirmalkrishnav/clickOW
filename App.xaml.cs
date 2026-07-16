using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ClickOw.Settings;
using Hardcodet.Wpf.TaskbarNotification;

namespace ClickOw;

/// <summary>
/// Application entry point. Runs tray-only: no main window is shown. Owns the tray
/// icon, settings and the <see cref="AppCoordinator"/> that drives the overlay.
/// </summary>
public partial class App : Application
{
    private Mutex? _singleInstanceMutex;
    private TaskbarIcon? _trayIcon;
    private AppSettings? _settings;
    private AppCoordinator? _coordinator;
    private SettingsWindow? _settingsWindow;

    private MenuItem? _enabledItem;
    private MenuItem? _laserItem;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Enforce a single running instance.
        _singleInstanceMutex = new Mutex(initiallyOwned: true, "ClickOw.SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            Shutdown();
            return;
        }

        _settings = SettingsStore.Load();
        StartupManager.Apply(_settings.RunAtStartup);

        _coordinator = new AppCoordinator(_settings);
        _coordinator.StateChanged += (_, _) => Dispatcher.Invoke(UpdateMenuState);

        BuildTrayIcon();
        UpdateMenuState();
    }

    private void BuildTrayIcon()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "ClickOw",
            Icon = LoadTrayIcon(),
        };

        var menu = new ContextMenu();

        _enabledItem = new MenuItem { Header = "Enabled  (Ctrl+Alt+C)", IsCheckable = true };
        _enabledItem.Click += (_, _) => _coordinator?.ToggleEnabled();

        _laserItem = new MenuItem { Header = "Laser pointer  (Ctrl+Alt+L)", IsCheckable = true };
        _laserItem.Click += (_, _) => _coordinator?.ToggleLaser();

        var settingsItem = new MenuItem { Header = "Settings\u2026" };
        settingsItem.Click += (_, _) => ShowSettings();

        var quitItem = new MenuItem { Header = "Quit ClickOw" };
        quitItem.Click += (_, _) => Shutdown();

        menu.Items.Add(_enabledItem);
        menu.Items.Add(_laserItem);
        menu.Items.Add(new Separator());
        menu.Items.Add(settingsItem);
        menu.Items.Add(new Separator());
        menu.Items.Add(quitItem);

        _trayIcon.ContextMenu = menu;
        _trayIcon.TrayMouseDoubleClick += (_, _) => ShowSettings();
    }

    private void UpdateMenuState()
    {
        if (_enabledItem is not null && _coordinator is not null)
        {
            _enabledItem.IsChecked = _coordinator.Enabled;
        }

        if (_laserItem is not null && _coordinator is not null)
        {
            _laserItem.IsChecked = _coordinator.LaserMode;
        }
    }

    private void ShowSettings()
    {
        if (_settings is null)
        {
            return;
        }

        if (_settingsWindow is { IsLoaded: true })
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow(_settings);
        _settingsWindow.Closed += (_, _) =>
        {
            _settingsWindow = null;
            UpdateMenuState();
        };
        _settingsWindow.Show();
        _settingsWindow.Activate();
    }

    private static System.Drawing.Icon LoadTrayIcon()
    {
        // Prefer the packaged resource; fall back to a system icon if missing.
        var uri = new Uri("pack://application:,,,/Assets/tray.ico", UriKind.Absolute);
        try
        {
            var info = Application.GetResourceStream(uri);
            if (info is not null)
            {
                using var stream = info.Stream;
                return new System.Drawing.Icon(stream);
            }
        }
        catch
        {
            // fall through
        }

        return System.Drawing.SystemIcons.Application;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _coordinator?.Dispose();
        _trayIcon?.Dispose();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}

