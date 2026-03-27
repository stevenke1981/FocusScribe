# FocusScribe Implementation Plan

## Summary

Build a new `FocusScribe` Windows desktop app as an unpackaged `WinUI 3 Blazor` application on `.NET 10` and `x64`. The app acts as a background transcription helper: capture microphone audio from a global hotkey, upload the recorded WAV file to `http://192.168.80.58:9000/v1/audio/transcriptions`, then paste the recognized text back into the previously focused external window.

## Architecture

- App shell: WinUI host with Blazor Web UI, single main window, tray-first behavior.
- Packaging: unpackaged (`WindowsPackageType=None`) so local CLI build/run and direct `.exe` workflows remain simple.
- Audio path: microphone capture to temporary PCM WAV file, then multipart upload.
- Delivery path: remember the foreground HWND before recording, restore focus best-effort, paste using clipboard and `Ctrl+V`.
- Persistence: JSON files under `%LocalAppData%\\FocusScribe\\`.

## Core Features

### 1. App Shell and Navigation

- Keep one main window and a simple navigation model: `Home`, `History`, `Settings`.
- Close action minimizes to tray by default instead of terminating the process.
- Show current status clearly: idle, recording, transcribing, sending, error.

### 2. Recording Workflow

- Register a global hotkey with default `Ctrl+Alt+Space`.
- First hotkey press:
  - capture the current foreground window handle and title
  - start microphone recording
  - update UI and tray tooltip/state
- Second hotkey press:
  - stop recording
  - flush the WAV file
  - submit the file to the transcription API

### 3. API Integration

- Base URL default: `http://192.168.80.58:9000`.
- Read service readiness from:
  - `GET /healthz`
  - `GET /v1/models`
- Submit transcription requests to `POST /v1/audio/transcriptions` as `multipart/form-data`.
- Send these request fields when present:
  - `file`
  - `model`
  - `language`
  - `punctuation`
  - `prompt`
  - `response_format=json`
- Parse success responses defensively because the OpenAPI document does not define the response schema. Prefer `text`; if absent, keep raw JSON and surface a parse error instead of silently dropping the result.

### 4. Text Delivery

- Primary send mode: clipboard paste.
- Delivery sequence:
  - snapshot current clipboard content if possible
  - write transcription text to clipboard
  - restore focus to the captured foreground window
  - send `Ctrl+V`
  - restore prior clipboard content after a short delay
- If focus restore fails:
  - keep the text available in clipboard/history
  - show a visible warning in the app and tray notification

### 5. History and Settings

- Keep the most recent 20 transcription records locally.
- Each record stores:
  - timestamp
  - target window title
  - transcript text
  - language/model used
  - raw response JSON
- Settings page includes:
  - server base URL
  - selected model
  - language override
  - punctuation toggle
  - optional prompt
  - hotkey selection

## Code Structure

- `Services/`
  - `SettingsStore`
  - `HistoryStore`
  - `TranscriptionClient`
  - `TranscriptionCoordinator`
- `Services/Native/`
  - `AudioCaptureService`
  - `GlobalHotkeyService`
  - `FocusedWindowBridge`
  - `ClipboardPasteService`
  - `TrayIconService`
- `Models/`
  - `AppSettings`
  - `TranscriptionRecord`
  - `TranscriptionResult`
  - `ServiceHealth`
- `Pages/`
  - `Home.razor`
  - `History.razor`
  - `Settings.razor`

## Library Choices

- Audio capture: `NAudio`
- Tray icon: `H.NotifyIcon.WinUI`
- HTTP: built-in `HttpClient`
- JSON: built-in `System.Text.Json`

## Windows-Specific Rules

- Use unpackaged-safe storage paths; do not rely on package identity APIs.
- Hotkey registration and tray lifetime belong in the native host layer, not in Razor components.
- Treat foreground restoration as best effort only; Windows may deny focus stealing.
- Use clipboard paste as the default delivery mode because it is more reliable for long text than simulated per-character typing.
- Always preserve user state when possible:
  - restore clipboard
  - preserve the last focused window identity
  - do not terminate on main window close

## Testing

### Functional

- App launches and shows a usable top-level window.
- Tray icon appears and can reopen the main window.
- Global hotkey starts and stops recording.
- A valid WAV file is created and uploaded successfully.
- Model list loads from `/v1/models`.
- Health status loads from `/healthz`.
- Transcribed text is pasted into Notepad and a browser text field.

### Failure Cases

- Server offline
- Empty or too-short recording
- API returns non-200
- API returns JSON without `text`
- Focus restore fails
- Clipboard access fails
- Hotkey registration conflict

### Persistence

- Settings survive restart.
- History survives restart.

## Acceptance Criteria

- User can press one global hotkey to start recording and the same hotkey to stop.
- The app submits the audio to the configured transcription server.
- The recognized text is pasted back into the previously focused window in the common case.
- Failures are visible and recoverable without restarting the app.

## Assumptions

- The current repo starts effectively empty except for the scaffolded `FocusScribe` project.
- The transcription service remains reachable on the local network at `192.168.80.58:9000`.
- The service currently reports model `CohereLabs/cohere-transcribe-03-2026`.
- v1 does not include streaming ASR, VAD auto-segmentation, file import, or startup-at-login.
