---
name: focusscribe-windows-integration
description: Use when building or reviewing the FocusScribe Windows desktop app, especially WinUI/Blazor host behavior, global hotkeys, tray lifetime, microphone capture, foreground-window restoration, clipboard paste delivery, or unpackaged Windows app constraints.
---

# FocusScribe Windows Integration

Use this skill for native Windows integration work in the FocusScribe app.

## Scope

- WinUI host lifecycle
- unpackaged Windows app behavior
- global hotkey registration
- tray-first lifetime
- foreground window tracking
- clipboard-based text delivery
- microphone recording and temporary file handling

## Default Decisions

- Target stack: unpackaged `WinUI 3 Blazor` on `.NET 10`
- Main workflow: one global hotkey toggles record/start and record/stop
- Output mode: clipboard paste, not per-character typing
- Persistence path: `%LocalAppData%\\FocusScribe\\`

## Required Workflow

1. Keep Windows-only behavior in native C# services, not inside Razor UI components.
2. Capture the current foreground window before starting audio recording.
3. Record audio to a temporary WAV file first, then upload the completed file.
4. Parse transcription responses defensively because the API success schema is not fixed.
5. Paste text back using clipboard plus `Ctrl+V`, then restore clipboard contents when possible.
6. Treat focus restoration as best effort and surface failure clearly in the UI.
7. Verify build and launch after native-host changes; do not stop at a successful compile.

## Windows Rules

- Do not assume package identity. Avoid APIs that only work for packaged apps unless guarded.
- Do not put hotkey registration into short-lived page objects; it must survive navigation and window hide/show.
- Do not depend on the main window staying open. Closing should minimize to tray unless the user explicitly exits.
- Do not assume `SetForegroundWindow` will always succeed; Windows focus-stealing rules can block it.
- Do not overwrite the clipboard without attempting restoration.
- Do not type long transcripts key-by-key by default; clipboard paste is the baseline path.
- Do not keep microphone files permanently unless the feature explicitly requires it.

## Implementation Notes

### Hotkeys

- Use `RegisterHotKey` with a stable native host lifetime.
- Provide a visible error when the chosen hotkey is already registered by another app.
- Default hotkey: `Ctrl+Alt+Space`.

### Foreground Window Return

- Store HWND and a human-readable window title before recording starts.
- Before paste, try to restore that window to foreground.
- If restore fails, keep the transcript in history and clipboard, then notify the user.

### Clipboard Paste

- Save the current clipboard payload best effort.
- Write transcript text to clipboard.
- Send `Ctrl+V`.
- Restore the previous clipboard payload after a short delay.
- If clipboard access fails, do not discard the transcript.

### Audio Capture

- Prefer WAV/PCM temporary files for simplest API compatibility.
- Keep capture start/stop state explicit to avoid duplicate stop calls.
- Reject empty or trivially short captures before upload when possible.

### API Calls

- Default base URL: `http://192.168.80.58:9000`
- Health check: `GET /healthz`
- Model discovery: `GET /v1/models`
- Transcription: `POST /v1/audio/transcriptions`
- Success responses may vary. Prefer `text`, otherwise retain raw JSON for diagnostics.

## Verification Checklist

- App opens a real top-level window.
- Tray icon appears and can reopen the window.
- Global hotkey toggles recording.
- Recorded file uploads successfully.
- Transcript can be pasted into Notepad.
- Clipboard is restored after paste in the normal case.
- Offline server and focus-restore failures are visible to the user.
