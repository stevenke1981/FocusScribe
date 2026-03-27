using System.ComponentModel;
using System.Runtime.InteropServices;
using FocusScribe.Interop;
using FocusScribe.Models;

namespace FocusScribe.Services.Native;

public sealed class GlobalHotkeyService : IDisposable
{
    private const int HotkeyId = 0x4653;

    private nint windowHandle;
    private nint previousWindowProc;
    private NativeMethods.WindowProc? currentWindowProc;
    private bool isInitialized;
    private bool isRegistered;

    public event EventHandler? Pressed;

    public void Initialize(nint hwnd)
    {
        if (isInitialized || hwnd == nint.Zero)
        {
            return;
        }

        windowHandle = hwnd;
        currentWindowProc = HandleWindowMessage;
        previousWindowProc = NativeMethods.SetWindowLongPtr(windowHandle, NativeMethods.GwlpWndProc, Marshal.GetFunctionPointerForDelegate(currentWindowProc));
        isInitialized = true;
    }

    public void RegisterOrThrow(HotkeySettings hotkey)
    {
        if (!isInitialized)
        {
            throw new InvalidOperationException("The hotkey service must be initialized with a window handle before registration.");
        }

        if (isRegistered)
        {
            NativeMethods.UnregisterHotKey(windowHandle, HotkeyId);
            isRegistered = false;
        }

        if (!NativeMethods.RegisterHotKey(windowHandle, HotkeyId, hotkey.ToNativeModifiers(), hotkey.ToVirtualKeyCode()))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Unable to register hotkey {hotkey}.");
        }

        isRegistered = true;
    }

    public void Dispose()
    {
        if (windowHandle != nint.Zero && isRegistered)
        {
            NativeMethods.UnregisterHotKey(windowHandle, HotkeyId);
        }

        if (windowHandle != nint.Zero && previousWindowProc != nint.Zero)
        {
            NativeMethods.SetWindowLongPtr(windowHandle, NativeMethods.GwlpWndProc, previousWindowProc);
        }
    }

    private nint HandleWindowMessage(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == NativeMethods.WmHotKey && wParam == HotkeyId)
        {
            Pressed?.Invoke(this, EventArgs.Empty);
            return 0;
        }

        return NativeMethods.CallWindowProc(previousWindowProc, hWnd, msg, wParam, lParam);
    }
}
