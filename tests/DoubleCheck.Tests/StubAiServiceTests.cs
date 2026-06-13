using DoubleCheck.Exceptions;
using DoubleCheck.Services.Ai;
using Xunit;

namespace DoubleCheck.Tests;

/// <summary>Service-layer example (Bekim): pure logic, no DB. Happy + sad.</summary>
public class StubAiServiceTests
{
    [Fact]
    public async Task GenerateAnswerAsync_WithValidQuestion_ReturnsAnswerMentioningCategory()
    {
        // Arrange
        var sut = new StubAiService();

        // Act
        var answer = await sut.GenerateAnswerAsync("How do I cook rice?", "Food");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(answer));
        Assert.Contains("Food", answer);
    }

    [Fact]
    public async Task GenerateAnswerAsync_WithEmptyQuestion_ThrowsValidationException()
    {
        // Arrange
        var sut = new StubAiService();

        // Act + Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.GenerateAnswerAsync("   ", "Food"));
    }
}
