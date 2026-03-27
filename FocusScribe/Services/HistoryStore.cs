using System.Text.Json;
using FocusScribe.Models;

namespace FocusScribe.Services;

public sealed class HistoryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string historyPath;

    public HistoryStore()
    {
        var appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FocusScribe");

        Directory.CreateDirectory(appDataDirectory);
        historyPath = Path.Combine(appDataDirectory, "history.json");
    }

    public async Task<IReadOnlyList<TranscriptionRecord>> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(historyPath))
        {
            return [];
        }

        await using var stream = File.OpenRead(historyPath);
        var history = await JsonSerializer.DeserializeAsync<List<TranscriptionRecord>>(stream, JsonOptions, cancellationToken);
        return history ?? [];
    }

    public async Task SaveAsync(IEnumerable<TranscriptionRecord> history, CancellationToken cancellationToken = default)
    {
        await using var stream = File.Create(historyPath);
        await JsonSerializer.SerializeAsync(stream, history, JsonOptions, cancellationToken);
    }
}
