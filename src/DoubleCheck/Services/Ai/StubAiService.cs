using DoubleCheck.Abstractions;
using DoubleCheck.Exceptions;

namespace DoubleCheck.Services.Ai;

/// <summary>Deterministic AI for deployment + tests. Bekim: extend as needed.</summary>
public class StubAiService : IAiService
{
    public Task<string> GenerateAnswerAsync(string question, string categoryName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ValidationException("Question must not be empty.");

        var answer = $"[AI/{categoryName}] Suggested answer to: \"{question.Trim()}\". " +
                     "This is an automated response; use Double Check for expert verification.";
        return Task.FromResult(answer);
    }
}
