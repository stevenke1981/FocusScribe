namespace FocusScribe.Models;

public sealed class AppSettings
{
    public string UiCulture { get; set; } = "zh-TW";

    public string BaseUrl { get; set; } = "http://192.168.80.58:9000";

    public string SelectedModel { get; set; } = "CohereLabs/cohere-transcribe-03-2026";

    public string Language { get; set; } = string.Empty;

    public bool Punctuation { get; set; } = true;

    public string Prompt { get; set; } = string.Empty;

    public HotkeySettings Hotkey { get; set; } = HotkeySettings.CreateDefault();
}
