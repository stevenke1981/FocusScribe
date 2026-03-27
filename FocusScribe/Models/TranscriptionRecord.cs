namespace FocusScribe.Models;

public sealed class TranscriptionRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;

    public string TargetWindowTitle { get; init; } = string.Empty;

    public string TranscriptText { get; init; } = string.Empty;

    public string Language { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public string RawResponseJson { get; init; } = string.Empty;
}
