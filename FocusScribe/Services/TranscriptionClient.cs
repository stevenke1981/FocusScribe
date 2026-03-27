using System.Net.Http.Headers;
using System.Text.Json;
using FocusScribe.Models;

namespace FocusScribe.Services;

public sealed class TranscriptionClient(HttpClient httpClient)
{
    public async Task<ServiceHealth> GetHealthAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync($"{baseUrl.TrimEnd('/')}/healthz", cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ServiceHealth
                {
                    IsHealthy = false,
                    Status = "offline",
                    ErrorMessage = payload
                };
            }

            using var json = JsonDocument.Parse(payload);
            var root = json.RootElement;

            return new ServiceHealth
            {
                IsHealthy = string.Equals(root.TryGetProperty("status", out var statusNode) ? statusNode.GetString() : "ok", "ok", StringComparison.OrdinalIgnoreCase),
                Status = root.TryGetProperty("status", out statusNode) ? statusNode.GetString() ?? "ok" : "ok",
                ModelId = root.TryGetProperty("model_id", out var modelNode) ? modelNode.GetString() ?? string.Empty : string.Empty,
                Device = root.TryGetProperty("device", out var deviceNode) ? deviceNode.GetString() ?? string.Empty : string.Empty,
                DefaultLanguage = root.TryGetProperty("default_language", out var languageNode) ? languageNode.GetString() ?? string.Empty : string.Empty
            };
        }
        catch (Exception ex)
        {
            return new ServiceHealth
            {
                IsHealthy = false,
                Status = "offline",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<IReadOnlyList<string>> GetModelsAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"{baseUrl.TrimEnd('/')}/v1/models", cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!json.RootElement.TryGetProperty("data", out var dataNode) || dataNode.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var models = new List<string>();

        foreach (var modelNode in dataNode.EnumerateArray())
        {
            if (modelNode.TryGetProperty("id", out var idNode))
            {
                var id = idNode.GetString();
                if (!string.IsNullOrWhiteSpace(id))
                {
                    models.Add(id);
                }
            }
        }

        return models;
    }

    public async Task<TranscriptionResult> CreateTranscriptionAsync(AppSettings settings, string audioFilePath, CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        await using var fileStream = File.OpenRead(audioFilePath);
        using var audioContent = new StreamContent(fileStream);
        audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
        form.Add(audioContent, "file", Path.GetFileName(audioFilePath));

        AddStringField(form, "model", settings.SelectedModel);
        AddStringField(form, "language", settings.Language);
        AddStringField(form, "prompt", settings.Prompt);
        AddStringField(form, "response_format", "json");
        form.Add(new StringContent(settings.Punctuation ? "true" : "false"), "punctuation");

        using var response = await httpClient.PostAsync($"{settings.BaseUrl.TrimEnd('/')}/v1/audio/transcriptions", form, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new TranscriptionResult
            {
                Success = false,
                RawResponseJson = payload,
                ErrorMessage = $"Transcription failed: {(int)response.StatusCode} {response.ReasonPhrase}"
            };
        }

        try
        {
            using var json = JsonDocument.Parse(payload);
            var transcript = ExtractTranscript(json.RootElement);

            return string.IsNullOrWhiteSpace(transcript)
                ? new TranscriptionResult
                {
                    Success = false,
                    RawResponseJson = payload,
                    ErrorMessage = "The server returned JSON without a usable transcript field."
                }
                : new TranscriptionResult
                {
                    Success = true,
                    TranscriptText = transcript,
                    RawResponseJson = payload
                };
        }
        catch (JsonException)
        {
            return new TranscriptionResult
            {
                Success = false,
                RawResponseJson = payload,
                ErrorMessage = "The server response was not valid JSON."
            };
        }
    }

    private static void AddStringField(MultipartFormDataContent form, string fieldName, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            form.Add(new StringContent(value), fieldName);
        }
    }

    private static string ExtractTranscript(JsonElement root)
    {
        foreach (var propertyName in new[] { "text", "transcript", "content", "output_text" })
        {
            if (root.TryGetProperty(propertyName, out var node) && node.ValueKind == JsonValueKind.String)
            {
                return node.GetString() ?? string.Empty;
            }
        }

        if (root.TryGetProperty("segments", out var segmentsNode) && segmentsNode.ValueKind == JsonValueKind.Array)
        {
            var parts = new List<string>();

            foreach (var segment in segmentsNode.EnumerateArray())
            {
                if (segment.TryGetProperty("text", out var textNode) && textNode.ValueKind == JsonValueKind.String)
                {
                    var text = textNode.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        parts.Add(text.Trim());
                    }
                }
            }

            return string.Join(" ", parts);
        }

        return string.Empty;
    }
}
