namespace FocusScribe.Models;

public sealed class TranscriptionResult
{
    public bool Success { get; init; }

    public string TranscriptText { get; init; } = string.Empty;

    public string RawResponseJson { get; init; } = string.Empty;

    public string ErrorMessage { get; init; } = string.Empty;
}
