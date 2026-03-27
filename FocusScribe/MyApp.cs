using System.Reflection;

namespace FocusScribe;

public partial class MyApp : Microsoft.Maui.Controls.Application
{
    public MyApp()
    {
        Resources.Add("Primary", Color.Parse("#f8c04a"));
        Resources.Add(nameof(VersionTemplate), new ControlTemplate(typeof(VersionTemplate)));
    }

    public static string MauiVersion
    {
        get
        {
            var version = typeof(MauiApp).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
            return $".NET MAUI ver. {version[..version.IndexOf('+')]}";
        }
    }
}
