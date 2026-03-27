using FocusScribe.Services.Native;
using H.NotifyIcon;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using WinUIWindow = Microsoft.UI.Xaml.Window;

namespace FocusScribe.Services;

public sealed class DesktopWindowService : IDisposable
{
    private readonly GlobalHotkeyService hotkeyService;
    private readonly TrayIconService trayIconService;
    private WinUIWindow? window;
    private bool allowExit;

    public DesktopWindowService(GlobalHotkeyService hotkeyService, TrayIconService trayIconService)
    {
        this.hotkeyService = hotkeyService;
        this.trayIconService = trayIconService;
    }

    public nint WindowHandle { get; private set; }

    public void Initialize(WinUIWindow appWindow)
    {
        if (window is not null)
        {
            return;
        }

        window = appWindow;
        window.Title = "FocusScribe";
        WindowHandle = WindowNative.GetWindowHandle(window);
        hotkeyService.Initialize(WindowHandle);
        trayIconService.Initialize(ShowMainWindow, ExitApplication);
        trayIconService.UpdateToolTip("Ready");

        window.AppWindow.Closing += OnAppWindowClosing;
    }

    public void ShowMainWindow()
    {
        if (window is null)
        {
            return;
        }

        H.NotifyIcon.WindowExtensions.Show(window);
        window.Activate();
    }

    public void HideMainWindow()
    {
        if (window is null)
        {
            return;
        }

        trayIconService.UpdateToolTip("Running in tray");
        H.NotifyIcon.WindowExtensions.Hide(window);
    }

    public void ExitApplication()
    {
        allowExit = true;
        trayIconService.Dispose();
        hotkeyService.Dispose();
        Microsoft.UI.Xaml.Application.Current.Exit();
    }

    public void Dispose()
    {
        trayIconService.Dispose();
        hotkeyService.Dispose();
    }

    private void OnAppWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (allowExit)
        {
            return;
        }

        args.Cancel = true;
        HideMainWindow();
    }
}
