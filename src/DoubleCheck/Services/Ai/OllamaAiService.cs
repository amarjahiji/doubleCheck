using System.Net.Http.Json;
using DoubleCheck.Abstractions;
using DoubleCheck.Exceptions;

namespace DoubleCheck.Services.Ai;

/// <summary>Generates AI answers through the local Ollama HTTP API.</summary>
public class OllamaAiService : IAiService
{
    private readonly HttpClient _http;
    private readonly string _model;

    /// <summary>Creates an Ollama-backed AI service from HTTP and configuration dependencies.</summary>
    public OllamaAiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _model = config["Ai:Model"] ?? "llama3.2";
        _http.BaseAddress = new Uri(config["Ai:OllamaUrl"] ?? "http://localhost:11434");
    }

    /// <inheritdoc />
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
            if (!resp.IsSuccessStatusCode)
                throw new BadGatewayException("AI provider returned an unsuccessful response.");

            var payload = await resp.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: ct);
            return payload?.Response?.Trim() ?? throw new DomainException("Empty response from Ollama.");
        }
        catch (HttpRequestException)
        {
            throw new BadGatewayException("AI provider is unavailable.");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new BadGatewayException("AI provider timed out.");
        }
    }

    private sealed class OllamaResponse { public string? Response { get; set; } }
}
