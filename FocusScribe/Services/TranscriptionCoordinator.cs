using FocusScribe.Models;
using FocusScribe.Services.Native;

namespace FocusScribe.Services;

public sealed class TranscriptionCoordinator : IDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly FocusScribeState state;
    private readonly UiLocalizer uiLocalizer;
    private readonly SettingsStore settingsStore;
    private readonly HistoryStore historyStore;
    private readonly TranscriptionClient transcriptionClient;
    private readonly AudioCaptureService audioCaptureService;
    private readonly FocusedWindowBridge focusedWindowBridge;
    private readonly ClipboardPasteService clipboardPasteService;
    private readonly GlobalHotkeyService globalHotkeyService;
    private readonly TrayIconService trayIconService;
    private readonly DesktopWindowService desktopWindowService;

    private FocusedWindowSnapshot? recordingTarget;
    private bool initialized;

    public TranscriptionCoordinator(
        FocusScribeState state,
        UiLocalizer uiLocalizer,
        SettingsStore settingsStore,
        HistoryStore historyStore,
        TranscriptionClient transcriptionClient,
        AudioCaptureService audioCaptureService,
        FocusedWindowBridge focusedWindowBridge,
        ClipboardPasteService clipboardPasteService,
        GlobalHotkeyService globalHotkeyService,
        TrayIconService trayIconService,
        DesktopWindowService desktopWindowService)
    {
        this.state = state;
        this.uiLocalizer = uiLocalizer;
        this.settingsStore = settingsStore;
        this.historyStore = historyStore;
        this.transcriptionClient = transcriptionClient;
        this.audioCaptureService = audioCaptureService;
        this.focusedWindowBridge = focusedWindowBridge;
        this.clipboardPasteService = clipboardPasteService;
        this.globalHotkeyService = globalHotkeyService;
        this.trayIconService = trayIconService;
        this.desktopWindowService = desktopWindowService;

        globalHotkeyService.Pressed += OnGlobalHotkeyPressed;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (initialized)
        {
            return;
        }

        var settings = await settingsStore.LoadAsync(cancellationToken);
        var history = await historyStore.LoadAsync(cancellationToken);

        state.SetSettings(settings);
        state.SetHistory(history);
        uiLocalizer.SetCulture(settings.UiCulture);

        ApplyHotkey(settings.Hotkey);
        await RefreshServiceStatusAsync(cancellationToken);
        state.SetReady(uiLocalizer.Get("Ready"), uiLocalizer.Get("ReadyForCapture"));
        trayIconService.UpdateToolTip("Ready");
        initialized = true;
    }

    public async Task RefreshServiceStatusAsync(CancellationToken cancellationToken = default)
    {
        var health = await transcriptionClient.GetHealthAsync(state.Settings.BaseUrl, cancellationToken);
        state.SetHealth(health);

        try
        {
            var models = await transcriptionClient.GetModelsAsync(state.Settings.BaseUrl, cancellationToken);
            state.SetModels(models);

            if (string.IsNullOrWhiteSpace(state.Settings.SelectedModel) && models.Count > 0)
            {
                state.Settings.SelectedModel = models[0];
            }
        }
        catch (Exception ex)
        {
            state.SetError(uiLocalizer.Get("ModelDiscoveryFailed"), ex.Message);
        }
    }

    public async Task SaveSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        state.SetSettings(settings);
        uiLocalizer.SetCulture(settings.UiCulture);
        ApplyHotkey(settings.Hotkey);
        await settingsStore.SaveAsync(settings, cancellationToken);
        await RefreshServiceStatusAsync(cancellationToken);
        state.SetInfo(uiLocalizer.Get("SettingsSaved"), $"Hotkey set to {settings.Hotkey}");
    }

    public async Task ToggleRecordingAsync(CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);

        try
        {
            if (state.IsRecording)
            {
                await StopRecordingAsync(cancellationToken);
                return;
            }

            if (state.IsBusy)
            {
                return;
            }

            recordingTarget = focusedWindowBridge.CaptureForegroundWindow();
            await audioCaptureService.StartAsync(cancellationToken);

            var targetTitle = string.IsNullOrWhiteSpace(recordingTarget.Title) ? "Current window" : recordingTarget.Title;
            state.SetRecording(targetTitle, uiLocalizer.Get("Recording"), uiLocalizer.Get("PressHotkeyAgain"));
            trayIconService.UpdateToolTip($"Recording into {targetTitle}");
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task PasteRecordIntoCurrentWindowAsync(TranscriptionRecord record, CancellationToken cancellationToken = default)
    {
        var currentTarget = focusedWindowBridge.CaptureForegroundWindow();
        var result = await clipboardPasteService.PasteTextAsync(record.TranscriptText, currentTarget, cancellationToken);
        state.SetInfo(uiLocalizer.Get("HistoryPasted"), result.Message);
    }

    public async Task PasteTextIntoCurrentWindowAsync(string transcript, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return;
        }

        var currentTarget = focusedWindowBridge.CaptureForegroundWindow();
        var result = await clipboardPasteService.PasteTextAsync(transcript, currentTarget, cancellationToken);
        state.SetInfo(uiLocalizer.Get("TranscriptPasted"), result.Message);
    }

    public void ShowWindow() => desktopWindowService.ShowMainWindow();

    public void HideWindow() => desktopWindowService.HideMainWindow();

    public void ExitApplication() => desktopWindowService.ExitApplication();

    public void Dispose()
    {
        globalHotkeyService.Pressed -= OnGlobalHotkeyPressed;
        gate.Dispose();
    }

    private async void OnGlobalHotkeyPressed(object? sender, EventArgs e)
    {
        try
        {
            await ToggleRecordingAsync();
        }
        catch (Exception ex)
        {
            state.SetError(uiLocalizer.Get("HotkeyActionFailed"), ex.Message);
        }
    }

    private void ApplyHotkey(HotkeySettings hotkey)
    {
        try
        {
            globalHotkeyService.RegisterOrThrow(hotkey);
        }
        catch (Exception ex)
        {
            state.SetError(uiLocalizer.Get("HotkeyRegistrationFailed"), ex.Message);
        }
    }

    private async Task StopRecordingAsync(CancellationToken cancellationToken)
    {
        state.SetBusy(uiLocalizer.Get("Transcribing"), uiLocalizer.Get("UploadingCapturedAudio"));
        trayIconService.UpdateToolTip("Transcribing");

        string audioPath = string.Empty;

        try
        {
            audioPath = await audioCaptureService.StopAsync(cancellationToken);

            var fileInfo = new FileInfo(audioPath);
            if (!fileInfo.Exists || fileInfo.Length < 1024)
            {
                state.SetError(uiLocalizer.Get("RecordingTooShort"), uiLocalizer.Get("NoUsefulAudio"));
                trayIconService.UpdateToolTip("Ready");
                return;
            }

            var transcription = await transcriptionClient.CreateTranscriptionAsync(state.Settings, audioPath, cancellationToken);
            if (!transcription.Success)
            {
                state.SetError(uiLocalizer.Get("TranscriptionFailed"), transcription.ErrorMessage);
                trayIconService.UpdateToolTip("Error");
                return;
            }

            var deliveryResult = await clipboardPasteService.PasteTextAsync(transcription.TranscriptText, recordingTarget, cancellationToken);

            var record = new TranscriptionRecord
            {
                TargetWindowTitle = recordingTarget?.Title ?? string.Empty,
                TranscriptText = transcription.TranscriptText,
                Language = state.Settings.Language,
                Model = state.Settings.SelectedModel,
                RawResponseJson = transcription.RawResponseJson
            };

            state.AddHistoryRecord(record);
            await historyStore.SaveAsync(state.History, cancellationToken);
            state.SetTranscript(
                transcription.TranscriptText,
                uiLocalizer.Get("Delivered"),
                uiLocalizer.Get("DeliveredDetail"),
                deliveryResult.Message);
            trayIconService.UpdateToolTip("Ready");
        }
        catch (Exception ex)
        {
            state.SetError(uiLocalizer.Get("CaptureFlowFailed"), ex.Message);
            trayIconService.UpdateToolTip("Error");
        }
        finally
        {
            recordingTarget = null;

            if (!string.IsNullOrWhiteSpace(audioPath) && File.Exists(audioPath))
            {
                try
                {
                    File.Delete(audioPath);
                }
                catch
                {
                }
            }
        }
    }
}
