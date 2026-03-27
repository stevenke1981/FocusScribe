using FocusScribe.Interop;
using FocusScribe.Models;

namespace FocusScribe.Services.Native;

public sealed class FocusedWindowBridge
{
    public FocusedWindowSnapshot CaptureForegroundWindow()
    {
        var handle = NativeMethods.GetForegroundWindow();
        return new FocusedWindowSnapshot
        {
            Handle = handle,
            Title = GetWindowTitle(handle)
        };
    }

    public bool TryRestore(FocusedWindowSnapshot? window)
    {
        if (window is null || window.Handle == nint.Zero)
        {
            return false;
        }

        NativeMethods.ShowWindow(window.Handle, NativeMethods.SwRestore);
        return NativeMethods.SetForegroundWindow(window.Handle);
    }

    private static string GetWindowTitle(nint handle)
    {
        if (handle == nint.Zero)
        {
            return string.Empty;
        }

        var length = NativeMethods.GetWindowTextLength(handle);
        if (length <= 0)
        {
            return string.Empty;
        }

        var buffer = new char[length + 1];
        _ = NativeMethods.GetWindowText(handle, buffer, buffer.Length);
        return new string(buffer).TrimEnd('\0');
    }
}
