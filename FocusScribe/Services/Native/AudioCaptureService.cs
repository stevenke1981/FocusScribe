using NAudio.Wave;

namespace FocusScribe.Services.Native;

public sealed class AudioCaptureService : IDisposable
{
    private readonly object syncRoot = new();
    private WaveInEvent? waveIn;
    private WaveFileWriter? writer;
    private TaskCompletionSource<string>? stopCompletion;
    private string activeFilePath = string.Empty;

    public bool IsRecording
    {
        get
        {
            lock (syncRoot)
            {
                return waveIn is not null;
            }
        }
    }

    public Task<string> StartAsync(CancellationToken cancellationToken = default)
    {
        lock (syncRoot)
        {
            if (waveIn is not null)
            {
                throw new InvalidOperationException("Audio capture is already running.");
            }

            activeFilePath = Path.Combine(Path.GetTempPath(), $"FocusScribe-{Guid.NewGuid():N}.wav");
            waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(16000, 16, 1),
                BufferMilliseconds = 120
            };

            writer = new WaveFileWriter(activeFilePath, waveIn.WaveFormat);
            stopCompletion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.RecordingStopped += OnRecordingStopped;
            waveIn.StartRecording();
        }

        return Task.FromResult(activeFilePath);
    }

    public Task<string> StopAsync(CancellationToken cancellationToken = default)
    {
        Task<string> pendingStop;

        lock (syncRoot)
        {
            if (waveIn is null || stopCompletion is null)
            {
                throw new InvalidOperationException("Audio capture is not running.");
            }

            pendingStop = stopCompletion.Task;
            waveIn.StopRecording();
        }

        return pendingStop;
    }

    public void Dispose()
    {
        lock (syncRoot)
        {
            DisposeWaveObjects();
        }
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        lock (syncRoot)
        {
            writer?.Write(e.Buffer, 0, e.BytesRecorded);
            writer?.Flush();
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        TaskCompletionSource<string>? completionSource;
        string filePath;

        lock (syncRoot)
        {
            completionSource = stopCompletion;
            filePath = activeFilePath;
            DisposeWaveObjects();
        }

        if (e.Exception is not null)
        {
            completionSource?.TrySetException(e.Exception);
            return;
        }

        completionSource?.TrySetResult(filePath);
    }

    private void DisposeWaveObjects()
    {
        waveIn?.Dispose();
        writer?.Dispose();
        waveIn = null;
        writer = null;
        stopCompletion = null;
    }
}
