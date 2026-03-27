using Windows.System;

namespace FocusScribe.Models;

public sealed class HotkeySettings
{
    public bool Ctrl { get; set; } = true;

    public bool Alt { get; set; } = true;

    public bool Shift { get; set; }

    public string Key { get; set; } = "Space";

    public static HotkeySettings CreateDefault() => new();

    public uint ToNativeModifiers()
    {
        uint modifiers = 0;

        if (Alt)
        {
            modifiers |= 0x0001;
        }

        if (Ctrl)
        {
            modifiers |= 0x0002;
        }

        if (Shift)
        {
            modifiers |= 0x0004;
        }

        return modifiers;
    }

    public uint ToVirtualKeyCode()
    {
        if (Enum.TryParse<VirtualKey>(Key, true, out var parsedKey))
        {
            return (uint)parsedKey;
        }

        return (uint)VirtualKey.Space;
    }

    public override string ToString()
    {
        var parts = new List<string>(4);

        if (Ctrl)
        {
            parts.Add("Ctrl");
        }

        if (Alt)
        {
            parts.Add("Alt");
        }

        if (Shift)
        {
            parts.Add("Shift");
        }

        parts.Add(Key);
        return string.Join("+", parts);
    }
}
