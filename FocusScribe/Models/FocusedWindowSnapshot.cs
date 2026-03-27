namespace FocusScribe.Models;

public sealed class FocusedWindowSnapshot
{
    public nint Handle { get; init; }

    public string Title { get; init; } = string.Empty;
}
