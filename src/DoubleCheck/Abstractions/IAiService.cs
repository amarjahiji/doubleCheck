namespace DoubleCheck.Abstractions;

/// <summary>AI answer generator. StubAiService (deploy/tests) or OllamaAiService (local),
/// selected via "Ai:Provider". Owned by Bekim.</summary>
public interface IAiService
{
    Task<string> GenerateAnswerAsync(string question, string categoryName, CancellationToken ct = default);
}
