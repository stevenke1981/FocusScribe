using FocusScribe.Interop;
using FocusScribe.Models;
using Microsoft.Maui.ApplicationModel;
using WindowsClipboard = Windows.ApplicationModel.DataTransfer.Clipboard;
using WindowsDataPackage = Windows.ApplicationModel.DataTransfer.DataPackage;
using WindowsDataFormats = Windows.ApplicationModel.DataTransfer.StandardDataFormats;

namespace FocusScribe.Services.Native;

public sealed class ClipboardPasteService(FocusedWindowBridge focusedWindowBridge)
{
    public async Task<TextDeliveryResult> PasteTextAsync(string text, FocusedWindowSnapshot? targetWindow, CancellationToken cancellationToken = default)
    {
        string? priorText = null;
        var hadText = false;

        try
        {
            (hadText, priorText) = await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var currentContent = WindowsClipboard.GetContent();
                if (currentContent.Contains(WindowsDataFormats.Text))
                {
                    return (true, await currentContent.GetTextAsync());
                }

                return (false, (string?)null);
            });

            await SetClipboardTextAsync(text);
        }
        catch (Exception ex)
        {
            return new TextDeliveryResult
            {
                FocusRestored = false,
                PasteAttempted = false,
                ClipboardRestored = false,
                Message = $"Clipboard access failed: {ex.Message}"
            };
        }

        var focusRestored = focusedWindowBridge.TryRestore(targetWindow);
        if (!focusRestored)
        {
            return new TextDeliveryResult
            {
                FocusRestored = false,
                PasteAttempted = false,
                ClipboardRestored = false,
                Message = "Could not restore the original target window. Transcript was left in the clipboard."
            };
        }

        SendPasteShortcut();
        await Task.Delay(180, cancellationToken);

        var clipboardRestored = false;

        try
        {
            if (hadText && priorText is not null)
            {
                await SetClipboardTextAsync(priorText);
                clipboardRestored = true;
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(WindowsClipboard.Clear);
                clipboardRestored = true;
            }
        }
        catch
        {
            clipboardRestored = false;
        }

        return new TextDeliveryResult
        {
            FocusRestored = true,
            PasteAttempted = true,
            ClipboardRestored = clipboardRestored,
            Message = clipboardRestored
                ? "Transcript pasted successfully."
                : "Transcript pasted, but the prior clipboard contents could not be restored."
        };
    }

    private static Task SetClipboardTextAsync(string text)
    {
        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            var dataPackage = new WindowsDataPackage();
            dataPackage.SetText(text);
            WindowsClipboard.SetContent(dataPackage);
            WindowsClipboard.Flush();
        });
    }

    private static void SendPasteShortcut()
    {
        var inputs = new[]
        {
            CreateKeyInput(0x11, false),
            CreateKeyInput(0x56, false),
            CreateKeyInput(0x56, true),
            CreateKeyInput(0x11, true)
        };

        NativeMethods.SendInput((uint)inputs.Length, inputs, System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.Input>());
    }

    private static NativeMethods.Input CreateKeyInput(ushort virtualKey, bool keyUp)
    {
        return new NativeMethods.Input
        {
            type = NativeMethods.InputKeyboard,
            U = new NativeMethods.InputUnion
            {
                ki = new NativeMethods.KeyboardInput
                {
                    wVk = virtualKey,
                    dwFlags = keyUp ? NativeMethods.KeyeventfKeyUp : 0
                }
            }
        };
    }
}
