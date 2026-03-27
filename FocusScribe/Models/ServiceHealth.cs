namespace FocusScribe.Models;

public sealed class ServiceHealth
{
    public bool IsHealthy { get; init; }

    public string Status { get; init; } = "unknown";

    public string ModelId { get; init; } = string.Empty;

    public string Device { get; init; } = string.Empty;

    public string DefaultLanguage { get; init; } = string.Empty;

    public string ErrorMessage { get; init; } = string.Empty;
}
