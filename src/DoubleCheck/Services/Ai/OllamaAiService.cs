using System.Net.Http.Json;
using DoubleCheck.Abstractions;
using DoubleCheck.Exceptions;

namespace DoubleCheck.Services.Ai;

/// <summary>Real local LLM via Ollama HTTP API (Ai:Provider=Ollama). Bekim: tune prompt/model.</summary>
public class OllamaAiService : IAiService
{
    private readonly HttpClient _http;
    private readonly string _model;

    public OllamaAiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _model = config["Ai:Model"] ?? "llama3.2";
        _http.BaseAddress = new Uri(config["Ai:OllamaUrl"] ?? "http://localhost:11434");
    }

    public async Task<string> GenerateAnswerAsync(string question, string categoryName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ValidationException("Question must not be empty.");

        var prompt = $"You are an assistant answering a question in the '{categoryName}' category. " +
                     $"Answer concisely.\n\nQuestion: {question}";
        try
        {
            var resp = await _http.PostAsJsonAsync("/api/generate",
                new { model = _model, prompt, stream = false }, ct);
            resp.EnsureSuccessStatusCode();
            var payload = await resp.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: ct);
            return payload?.Response?.Trim() ?? throw new DomainException("Empty response from Ollama.");
        }
        catch (HttpRequestException ex)
        {
            throw new DomainException($"AI provider unavailable: {ex.Message}");
        }
    }

    private sealed class OllamaResponse { public string? Response { get; set; } }
}
