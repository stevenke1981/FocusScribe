using FocusScribe.Infrastructure;
using H.NotifyIcon;
using WinUIColor = Windows.UI.Color;
using WinUIMenuFlyout = Microsoft.UI.Xaml.Controls.MenuFlyout;
using WinUIMenuFlyoutItem = Microsoft.UI.Xaml.Controls.MenuFlyoutItem;
using WinUIMenuFlyoutSeparator = Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator;
using WinUISolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WinUIFontFamily = Microsoft.UI.Xaml.Media.FontFamily;

namespace FocusScribe.Services.Native;

public sealed class TrayIconService : IDisposable
{
    private TaskbarIcon? trayIcon;

    public void Initialize(Action showWindowAction, Action exitAction)
    {
        if (trayIcon is not null)
        {
            return;
        }

        var menu = new WinUIMenuFlyout();

        var openItem = new WinUIMenuFlyoutItem
        {
            Text = "Open FocusScribe"
        };
        openItem.Click += (_, _) => showWindowAction();

        var exitItem = new WinUIMenuFlyoutItem
        {
            Text = "Exit"
        };
        exitItem.Click += (_, _) => exitAction();

        menu.Items.Add(openItem);
        menu.Items.Add(new WinUIMenuFlyoutSeparator());
        menu.Items.Add(exitItem);

        trayIcon = new TaskbarIcon
        {
            ToolTipText = "FocusScribe",
            LeftClickCommand = new DelegateCommand(showWindowAction),
            ContextFlyout = menu,
            IconSource = new GeneratedIconSource
            {
                Text = "FS",
                FontSize = 34,
                FontFamily = new WinUIFontFamily("Bahnschrift SemiBold"),
                Background = new WinUISolidColorBrush(WinUIColor.FromArgb(255, 248, 192, 74)),
                Foreground = new WinUISolidColorBrush(WinUIColor.FromArgb(255, 24, 28, 39))
            }
        };

        trayIcon.ForceCreate();
    }

    public void UpdateToolTip(string statusText)
    {
        if (trayIcon is not null)
        {
            trayIcon.ToolTipText = $"FocusScribe - {statusText}";
        }
    }

    public void Dispose()
    {
        trayIcon?.Dispose();
        trayIcon = null;
    }
}
