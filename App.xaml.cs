using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ClickOw.Settings;
using Hardcodet.Wpf.TaskbarNotification;

namespace ClickOw;

/// <summary>
/// Application entry point. Runs tray-only: no main window is shown. Owns the tray
/// icon, settings and the <see cref="AppCoordinator"/> that drives the overlay. Every
/// setting is exposed directly through the tray icon's right-click menu.
/// </summary>
public partial class App : Application
{
    private Mutex? _singleInstanceMutex;
    private TaskbarIcon? _trayIcon;
    private AppSettings? _settings;
    private AppCoordinator? _coordinator;

    // Base colors used when a color theme is set to Default; mirror AppSettings so the
    // menu swatches show the resolved color for each context.
    private const string ClickDefaultHex = "#FF3DA5FF";
    private const string DragDefaultHex = "#FFB68CFF";
    private const string LaserDefaultHex = "#FFFF4D4D";

    // All checkable menu items and the predicate that decides whether they are checked.
    private readonly List<(MenuItem Item, Func<bool> IsChecked)> _checkables = new();

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
        _coordinator.StateChanged += (_, _) => Dispatcher.Invoke(() =>
        {
            SettingsStore.Save(_settings);
            RefreshChecks();
        });

        BuildTrayIcon();
        RefreshChecks();
    }

    private void BuildTrayIcon()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "ClickOw",
            Icon = LoadTrayIcon(),
        };

        // Open the context menu on left click as well as the default right click.
        _trayIcon.TrayLeftMouseUp += (_, _) =>
        {
            if (_trayIcon.ContextMenu is { } contextMenu)
            {
                contextMenu.IsOpen = true;
            }
        };

        var menu = new ContextMenu();

        var enabledItem = new MenuItem
        {
            Header = "Enabled  (Ctrl+Alt+O)",
            IsCheckable = true,
            StaysOpenOnClick = true,
        };
        enabledItem.Click += (_, _) => _coordinator?.ToggleEnabled();
        Register(enabledItem, () => _coordinator?.Enabled == true);

        var laserItem = new MenuItem
        {
            Header = "Laser pointer  (Ctrl+Alt+L)",
            IsCheckable = true,
            StaysOpenOnClick = true,
        };
        laserItem.Click += (_, _) => ApplyChange(() =>
        {
            _coordinator?.ToggleLaser();
            // Laser and drag animation are mutually exclusive.
            if (_settings!.LaserMode)
            {
                _settings.DragAnimation = false;
            }
        });
        Register(laserItem, () => _coordinator?.LaserMode == true);

        // Drag animation sits under laser and is mutually exclusive with it.
        var dragItem = BuildToggle(
            "Drag animation",
            () => _settings!.DragAnimation,
            v =>
            {
                _settings!.DragAnimation = v;
                if (v)
                {
                    _settings.LaserMode = false;
                }
            });

        // Colors submenu: each context is a titled section of inline color choices.
        var colors = new MenuItem { Header = "Colors" };
        AddColorSection(colors.Items, "Click color", ClickDefaultHex,
            () => _settings!.ClickColor, v => _settings!.ClickColor = v);
        colors.Items.Add(new Separator());
        AddColorSection(colors.Items, "Drag color", DragDefaultHex,
            () => _settings!.DragColor, v => _settings!.DragColor = v);
        colors.Items.Add(new Separator());
        AddColorSection(colors.Items, "Laser color", LaserDefaultHex,
            () => _settings!.LaserColor, v => _settings!.LaserColor = v);

        // Sizes submenu: each context is a titled section of inline size choices.
        var sizes = new MenuItem { Header = "Sizes" };
        AddSizeSection(sizes.Items, "Click size",
            () => _settings!.ClickSizePreset, v => _settings!.ClickSizePreset = v);
        sizes.Items.Add(new Separator());
        AddSizeSection(sizes.Items, "Laser thickness",
            () => _settings!.LaserThicknessPreset, v => _settings!.LaserThicknessPreset = v);

        // Options submenu for remaining toggles.
        var options = new MenuItem { Header = "Options" };
        options.Items.Add(BuildToggle(
            "Run at Windows startup",
            () => _settings!.RunAtStartup, v => _settings!.RunAtStartup = v,
            afterChange: () => StartupManager.Apply(_settings!.RunAtStartup)));

        var quitItem = new MenuItem { Header = "Quit ClickOw" };
        quitItem.Click += (_, _) => Shutdown();

        var aboutItem = new MenuItem { Header = $"About ClickOw  V {GetAppVersion()}" };
        aboutItem.Click += (_, _) => OpenAboutPage();

        menu.Items.Add(enabledItem);
        menu.Items.Add(laserItem);
        menu.Items.Add(dragItem);
        menu.Items.Add(new Separator());
        menu.Items.Add(colors);
        menu.Items.Add(sizes);
        menu.Items.Add(options);
        menu.Items.Add(new Separator());
        menu.Items.Add(quitItem);
        menu.Items.Add(new Separator());
        menu.Items.Add(aboutItem);

        _trayIcon.ContextMenu = menu;
        WarmUpContextMenu(menu);
    }

    /// <summary>
    /// Realizes the context menu's popup off-screen once at idle so its first real
    /// open doesn't flicker (WPF builds the popup visual tree lazily on first show).
    /// </summary>
    private void WarmUpContextMenu(ContextMenu menu)
    {
        Dispatcher.BeginInvoke(
            new Action(() =>
            {
                menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Absolute;
                menu.HorizontalOffset = -10000;
                menu.VerticalOffset = -10000;
                menu.IsOpen = true;
                menu.IsOpen = false;
                menu.ClearValue(ContextMenu.PlacementProperty);
                menu.ClearValue(ContextMenu.HorizontalOffsetProperty);
                menu.ClearValue(ContextMenu.VerticalOffsetProperty);
            }),
            System.Windows.Threading.DispatcherPriority.ApplicationIdle);
    }

    private void AddColorSection(
        ItemCollection items, string title, string defaultHex,
        Func<ColorTheme> getter, Action<ColorTheme> setter)
    {
        items.Add(BuildHeader(title));
        foreach (ColorTheme theme in ColorPalette.All)
        {
            ColorTheme captured = theme;
            var item = new MenuItem
            {
                Header = FriendlyName(captured.ToString()),
                IsCheckable = true,
                StaysOpenOnClick = true,
                Icon = BuildSwatch(ColorPalette.ResolveHex(captured, defaultHex)),
            };
            item.Click += (_, _) => ApplyChange(() => setter(captured));
            Register(item, () => EqualityComparer<ColorTheme>.Default.Equals(getter(), captured));
            items.Add(item);
        }
    }

    private void AddSizeSection(
        ItemCollection items, string title, Func<SizePreset> getter, Action<SizePreset> setter)
    {
        items.Add(BuildHeader(title));
        foreach (SizePreset preset in (SizePreset[])Enum.GetValues(typeof(SizePreset)))
        {
            SizePreset captured = preset;
            var item = new MenuItem
            {
                Header = captured.ToString(),
                IsCheckable = true,
                StaysOpenOnClick = true,
            };
            item.Click += (_, _) => ApplyChange(() => setter(captured));
            Register(item, () => EqualityComparer<SizePreset>.Default.Equals(getter(), captured));
            items.Add(item);
        }
    }

    private static MenuItem BuildHeader(string text)
    {
        return new MenuItem
        {
            Header = text,
            IsEnabled = false,
            FontWeight = FontWeights.SemiBold,
        };
    }

    private MenuItem BuildToggle(
        string header, Func<bool> getter, Action<bool> setter, Action? afterChange = null)
    {
        var item = new MenuItem
        {
            Header = header,
            IsCheckable = true,
            StaysOpenOnClick = true,
        };
        item.Click += (_, _) => ApplyChange(() =>
        {
            setter(!getter());
            afterChange?.Invoke();
        });
        Register(item, getter);
        return item;
    }

    private void ApplyChange(Action change)
    {
        change();
        if (_settings is not null)
        {
            SettingsStore.Save(_settings);
        }

        RefreshChecks();
    }

    private void Register(MenuItem item, Func<bool> isChecked)
    {
        _checkables.Add((item, isChecked));
    }

    private void RefreshChecks()
    {
        foreach ((MenuItem item, Func<bool> isChecked) in _checkables)
        {
            item.IsChecked = isChecked();
        }
    }

    private static object BuildSwatch(string hex)
    {
        return new Rectangle
        {
            Width = 12,
            Height = 12,
            RadiusX = 2,
            RadiusY = 2,
            Fill = BrushFromHex(hex),
            Stroke = new SolidColorBrush(Color.FromArgb(0x40, 0, 0, 0)),
            StrokeThickness = 1,
        };
    }

    private static Brush BrushFromHex(string hex)
    {
        try
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }
        catch
        {
            return Brushes.Transparent;
        }
    }

    private static string FriendlyName(string pascalCase)
    {
        return Regex.Replace(pascalCase, "(\\B[A-Z])", " $1");
    }

    private static string GetAppVersion()
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        return version is null ? "1.0" : $"{version.Major}.{version.Minor}";
    }

    private static void OpenAboutPage()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/nirmalkrishnav/clickow",
                UseShellExecute = true,
            });
        }
        catch
        {
            // Ignore failures to launch the default browser.
        }
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

