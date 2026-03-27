namespace FocusScribe.Views;

public partial class HybridPage : Microsoft.UI.Xaml.Controls.Page
{
    private readonly Lazy<MauiContext> mauiContext = new(InitializeMauiContext);

    protected MauiContext MauiContext => mauiContext.Value;

    private static MauiContext InitializeMauiContext()
    {
        return new MauiContext(App.Services);
    }
}
