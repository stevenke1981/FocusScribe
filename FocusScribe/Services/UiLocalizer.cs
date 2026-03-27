using System.Globalization;

namespace FocusScribe.Services;

public sealed class UiLocalizer
{
    private static readonly Dictionary<string, Dictionary<string, string>> Catalog = new(StringComparer.OrdinalIgnoreCase)
    {
        ["zh-TW"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["AppSubtitle"] = "聚焦中的桌面轉錄",
            ["ServerHealthy"] = "服務正常",
            ["ServerAttention"] = "服務需處理",
            ["NavHome"] = "首頁",
            ["NavHistory"] = "歷史",
            ["NavSettings"] = "設定",
            ["HomeEyebrow"] = "目前焦點視窗的桌面聽寫",
            ["HomeHeroCopy"] = "在任何地方使用 {0} 開始錄音，將音訊送到 Cohere 轉錄服務，再把結果貼回你原本正在使用的視窗。",
            ["StartRecording"] = "開始錄音",
            ["StopAndTranscribe"] = "停止並轉錄",
            ["HideToTray"] = "縮到系統匣",
            ["MetricStatus"] = "狀態",
            ["MetricServer"] = "伺服器",
            ["MetricTargetWindow"] = "目標視窗",
            ["MetricHistory"] = "歷史紀錄",
            ["TargetWindowHelp"] = "錄音開始時會記住當前前景視窗。",
            ["HistoryHelp"] = "最近的轉錄結果會保存在本機，可供重送或檢查。",
            ["WaitingForCapture"] = "等待首次錄音",
            ["BannerError"] = "錯誤",
            ["BannerInfo"] = "資訊",
            ["LatestTranscriptEyebrow"] = "最近一次轉錄",
            ["LatestTranscriptTitle"] = "可再次貼送",
            ["PasteIntoCurrentWindow"] = "貼到目前視窗",
            ["TranscriptPlaceholder"] = "第一次成功轉錄後，最新結果會顯示在這裡。",
            ["HistoryEyebrow"] = "最近的本機轉錄",
            ["HistoryTitle"] = "歷史",
            ["NoTranscriptsTitle"] = "尚無轉錄紀錄",
            ["NoTranscriptsCopy"] = "先從熱鍵或首頁執行一次錄音，之後就會出現在這裡。",
            ["UnknownTarget"] = "未知目標",
            ["SettingsEyebrow"] = "伺服器與桌面行為",
            ["SettingsTitle"] = "設定",
            ["RefreshService"] = "重新整理服務",
            ["SaveSettings"] = "儲存設定",
            ["SettingsServerTitle"] = "轉錄伺服器",
            ["BaseUrl"] = "Base URL",
            ["Model"] = "模型",
            ["LanguageOverride"] = "語言覆寫",
            ["LanguageOverridePlaceholder"] = "留空代表自動",
            ["EnablePunctuation"] = "啟用標點",
            ["Prompt"] = "提示詞",
            ["PromptPlaceholder"] = "可選的轉錄提示詞",
            ["UiLanguage"] = "介面語言",
            ["SettingsHotkeyTitle"] = "全域熱鍵",
            ["HotkeyKey"] = "按鍵",
            ["CurrentShortcut"] = "目前快捷鍵",
            ["SettingsSnapshotTitle"] = "服務快照",
            ["Status"] = "狀態",
            ["Device"] = "裝置",
            ["DefaultLanguage"] = "預設語言",
            ["NotFound"] = "找不到這個頁面。",
            ["Ready"] = "就緒",
            ["ReadyForCapture"] = "隨時可以開始錄音",
            ["Recording"] = "錄音中",
            ["PressHotkeyAgain"] = "再次按下熱鍵即可停止並送出轉錄",
            ["Delivered"] = "已送出",
            ["DeliveredDetail"] = "轉錄已完成並處理",
            ["SettingsSaved"] = "設定已儲存",
            ["ModelDiscoveryFailed"] = "模型清單取得失敗",
            ["HotkeyRegistrationFailed"] = "熱鍵註冊失敗",
            ["HotkeyActionFailed"] = "熱鍵動作失敗",
            ["Transcribing"] = "轉錄中",
            ["UploadingCapturedAudio"] = "正在將錄到的音訊上傳至伺服器",
            ["RecordingTooShort"] = "錄音太短",
            ["NoUsefulAudio"] = "沒有錄到足夠可用的音訊。",
            ["TranscriptionFailed"] = "轉錄失敗",
            ["CaptureFlowFailed"] = "錄音流程失敗",
            ["HistoryPasted"] = "已從歷史貼送",
            ["TranscriptPasted"] = "已貼送轉錄文字"
        },
        ["en-US"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["AppSubtitle"] = "Focused desktop transcription",
            ["ServerHealthy"] = "Server Healthy",
            ["ServerAttention"] = "Server Needs Attention",
            ["NavHome"] = "Home",
            ["NavHistory"] = "History",
            ["NavSettings"] = "Settings",
            ["HomeEyebrow"] = "Desktop dictation for the active window",
            ["HomeHeroCopy"] = "Record from anywhere with {0}, send audio to your Cohere transcription server, then paste the result back into the window you were already using.",
            ["StartRecording"] = "Start Recording",
            ["StopAndTranscribe"] = "Stop And Transcribe",
            ["HideToTray"] = "Hide To Tray",
            ["MetricStatus"] = "Status",
            ["MetricServer"] = "Server",
            ["MetricTargetWindow"] = "Target Window",
            ["MetricHistory"] = "History",
            ["TargetWindowHelp"] = "The foreground window captured at recording start.",
            ["HistoryHelp"] = "Recent transcripts are stored locally for resend and inspection.",
            ["WaitingForCapture"] = "Waiting for first capture",
            ["BannerError"] = "Error",
            ["BannerInfo"] = "Info",
            ["LatestTranscriptEyebrow"] = "Latest Transcript",
            ["LatestTranscriptTitle"] = "Ready to resend",
            ["PasteIntoCurrentWindow"] = "Paste Into Current Window",
            ["TranscriptPlaceholder"] = "Your latest transcript will appear here after the first successful capture.",
            ["HistoryEyebrow"] = "Recent local transcripts",
            ["HistoryTitle"] = "History",
            ["NoTranscriptsTitle"] = "No transcripts yet",
            ["NoTranscriptsCopy"] = "Run a first recording from the hotkey or Home page and it will appear here.",
            ["UnknownTarget"] = "Unknown target",
            ["SettingsEyebrow"] = "Server and desktop behavior",
            ["SettingsTitle"] = "Settings",
            ["RefreshService"] = "Refresh Service",
            ["SaveSettings"] = "Save Settings",
            ["SettingsServerTitle"] = "Transcription Server",
            ["BaseUrl"] = "Base URL",
            ["Model"] = "Model",
            ["LanguageOverride"] = "Language Override",
            ["LanguageOverridePlaceholder"] = "Leave empty for auto",
            ["EnablePunctuation"] = "Enable punctuation",
            ["Prompt"] = "Prompt",
            ["PromptPlaceholder"] = "Optional transcription prompt",
            ["UiLanguage"] = "UI Language",
            ["SettingsHotkeyTitle"] = "Global Hotkey",
            ["HotkeyKey"] = "Key",
            ["CurrentShortcut"] = "Current shortcut",
            ["SettingsSnapshotTitle"] = "Service Snapshot",
            ["Status"] = "Status",
            ["Device"] = "Device",
            ["DefaultLanguage"] = "Default Language",
            ["NotFound"] = "Sorry, there's nothing at this address.",
            ["Ready"] = "Ready",
            ["ReadyForCapture"] = "Ready for capture",
            ["Recording"] = "Recording",
            ["PressHotkeyAgain"] = "Press the hotkey again to stop and transcribe",
            ["Delivered"] = "Delivered",
            ["DeliveredDetail"] = "Transcript captured and processed",
            ["SettingsSaved"] = "Settings saved",
            ["ModelDiscoveryFailed"] = "Model discovery failed",
            ["HotkeyRegistrationFailed"] = "Hotkey registration failed",
            ["HotkeyActionFailed"] = "Hotkey action failed",
            ["Transcribing"] = "Transcribing",
            ["UploadingCapturedAudio"] = "Uploading captured audio to the server",
            ["RecordingTooShort"] = "Recording too short",
            ["NoUsefulAudio"] = "No useful audio was captured.",
            ["TranscriptionFailed"] = "Transcription failed",
            ["CaptureFlowFailed"] = "Capture flow failed",
            ["HistoryPasted"] = "History pasted",
            ["TranscriptPasted"] = "Transcript pasted"
        }
    };

    public event Action? Changed;

    public string CurrentCulture { get; private set; } = "zh-TW";

    public IReadOnlyList<string> SupportedCultures => ["zh-TW", "en-US"];

    public string this[string key] => Get(key);

    public string Get(string key)
    {
        if (Catalog.TryGetValue(CurrentCulture, out var cultureMap) && cultureMap.TryGetValue(key, out var value))
        {
            return value;
        }

        if (Catalog["zh-TW"].TryGetValue(key, out var fallback))
        {
            return fallback;
        }

        return key;
    }

    public string Format(string key, params object[] args)
    {
        return string.Format(CultureInfo.CurrentCulture, Get(key), args);
    }

    public void SetCulture(string? cultureName)
    {
        var resolvedCulture = SupportedCultures.Contains(cultureName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            ? cultureName!
            : "zh-TW";

        var culture = CultureInfo.GetCultureInfo(resolvedCulture);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CurrentCulture = culture.Name;
        Changed?.Invoke();
    }
}
