using FocusScribe.Services;
using Microsoft.Maui.Hosting;
using WinUIApplication = Microsoft.UI.Xaml.Application;
using WinUIWindow = Microsoft.UI.Xaml.Window;

namespace FocusScribe;

public partial class App : WinUIApplication
{
    private static readonly Lazy<MauiApp> MauiAppFactory = new(MauiProgram.CreateMauiApp);
    private WinUIWindow? window;

    public App()
    {
        InitializeComponent();
    }

    public static MauiApp MauiApp => MauiAppFactory.Value;

    public static IServiceProvider Services => MauiApp.Services;

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs e)
    {
        window ??= new WinUIWindow();
        window.Content = new MainPage();
        window.Activate();

        var desktopWindowService = Services.GetRequiredService<DesktopWindowService>();
        desktopWindowService.Initialize(window);

        var coordinator = Services.GetRequiredService<TranscriptionCoordinator>();
        _ = coordinator.InitializeAsync();
    }
}
