namespace FocusScribe.Models;

public sealed class TextDeliveryResult
{
    public bool FocusRestored { get; init; }

    public bool PasteAttempted { get; init; }

    public bool ClipboardRestored { get; init; }

    public string Message { get; init; } = string.Empty;
}
