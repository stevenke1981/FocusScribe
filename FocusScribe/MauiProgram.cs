using FocusScribe.Services;
using FocusScribe.Services.Native;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Embedding;

namespace FocusScribe;

public static partial class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiEmbeddedApp<MyApp>()
               .ConfigureFonts(fonts =>
               {
                   fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                   fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
               });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton(AppInfo.Current);
        builder.Services.AddSingleton(new HttpClient());
        builder.Services.AddSingleton<FocusScribeState>();
        builder.Services.AddSingleton<UiLocalizer>();
        builder.Services.AddSingleton<SettingsStore>();
        builder.Services.AddSingleton<HistoryStore>();
        builder.Services.AddSingleton<TranscriptionClient>();
        builder.Services.AddSingleton<AudioCaptureService>();
        builder.Services.AddSingleton<FocusedWindowBridge>();
        builder.Services.AddSingleton<ClipboardPasteService>();
        builder.Services.AddSingleton<GlobalHotkeyService>();
        builder.Services.AddSingleton<TrayIconService>();
        builder.Services.AddSingleton<DesktopWindowService>();
        builder.Services.AddSingleton<TranscriptionCoordinator>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
