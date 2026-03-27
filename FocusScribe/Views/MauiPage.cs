using Microsoft.AspNetCore.Components.WebView.Maui;

namespace FocusScribe.Views;

public partial class MauiPage : ContentPage
{
    public MauiPage()
    {
        var blazorWebView = new BlazorWebView
        {
            StartPath = "/",
            HostPage = "wwwroot/index.html"
        };

        blazorWebView.RootComponents.Add(new RootComponent
        {
            Selector = "#app",
            ComponentType = typeof(Main)
        });

        Content = blazorWebView;
    }
}
