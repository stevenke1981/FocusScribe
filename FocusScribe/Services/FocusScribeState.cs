using FocusScribe.Models;

namespace FocusScribe.Services;

public sealed class FocusScribeState
{
    private List<string> availableModels = [];
    private List<TranscriptionRecord> history = [];

    public event Action? Changed;

    public AppSettings Settings { get; private set; } = new();

    public ServiceHealth Health { get; private set; } = new();

    public IReadOnlyList<string> AvailableModels => availableModels;

    public IReadOnlyList<TranscriptionRecord> History => history;

    public string StatusHeadline { get; private set; } = "啟動中";

    public string StatusDetail { get; private set; } = "正在準備 FocusScribe";

    public string ActiveTargetWindowTitle { get; private set; } = string.Empty;

    public string LatestTranscript { get; private set; } = string.Empty;

    public string LastError { get; private set; } = string.Empty;

    public string LastInfo { get; private set; } = string.Empty;

    public bool IsRecording { get; private set; }

    public bool IsBusy { get; private set; }

    public void SetSettings(AppSettings settings)
    {
        Settings = settings;
        NotifyChanged();
    }

    public void SetHealth(ServiceHealth health)
    {
        Health = health;
        NotifyChanged();
    }

    public void SetModels(IEnumerable<string> models)
    {
        availableModels = models.Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        NotifyChanged();
    }

    public void SetHistory(IEnumerable<TranscriptionRecord> records)
    {
        history = records.OrderByDescending(static record => record.CreatedAt).ToList();
        NotifyChanged();
    }

    public void AddHistoryRecord(TranscriptionRecord record)
    {
        history.Insert(0, record);
        history = history.Take(20).ToList();
        NotifyChanged();
    }

    public void SetReady(string headline, string detail)
    {
        IsBusy = false;
        IsRecording = false;
        StatusHeadline = headline;
        StatusDetail = detail;
        ActiveTargetWindowTitle = string.Empty;
        NotifyChanged();
    }

    public void SetRecording(string targetWindowTitle, string headline, string detail)
    {
        IsBusy = false;
        IsRecording = true;
        StatusHeadline = headline;
        StatusDetail = detail;
        ActiveTargetWindowTitle = targetWindowTitle;
        LastError = string.Empty;
        LastInfo = string.Empty;
        NotifyChanged();
    }

    public void SetBusy(string headline, string detail)
    {
        IsBusy = true;
        IsRecording = false;
        StatusHeadline = headline;
        StatusDetail = detail;
        LastError = string.Empty;
        NotifyChanged();
    }

    public void SetTranscript(string transcript, string headline, string detail, string infoMessage)
    {
        IsBusy = false;
        IsRecording = false;
        LatestTranscript = transcript;
        StatusHeadline = headline;
        StatusDetail = detail;
        LastInfo = infoMessage;
        LastError = string.Empty;
        NotifyChanged();
    }

    public void SetInfo(string headline, string detail)
    {
        StatusHeadline = headline;
        StatusDetail = detail;
        LastInfo = detail;
        NotifyChanged();
    }

    public void SetError(string headline, string detail)
    {
        IsBusy = false;
        IsRecording = false;
        StatusHeadline = headline;
        StatusDetail = detail;
        LastError = detail;
        NotifyChanged();
    }

    private void NotifyChanged() => Changed?.Invoke();
}
