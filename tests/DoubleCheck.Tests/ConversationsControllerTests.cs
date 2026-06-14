using DoubleCheck.Abstractions;
using DoubleCheck.Controllers;
using DoubleCheck.Dtos;
using DoubleCheck.Exceptions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace DoubleCheck.Tests;

public class ConversationsControllerTests
{
    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        // Arrange
        var chat = Substitute.For<IChatService>();
        var response = new ConversationResponse(Guid.NewGuid(), "Title", Guid.NewGuid(), "Tech", DateTime.UtcNow);
        chat.CreateConversationAsync(Arg.Any<CreateConversationRequest>(), Arg.Any<CancellationToken>()).Returns(response);
        var sut = new ConversationsController(chat);

        // Act
        var result = await sut.Create(new CreateConversationRequest { Title = "Title", CategoryId = response.CategoryId }, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ConversationsController.Get), created.ActionName);
        Assert.Same(response, created.Value);
    }

    [Fact]
    public async Task Get_WhenServiceRejectsOwnership_ThrowsForbidden()
    {
        // Arrange
        var chat = Substitute.For<IChatService>();
        chat.GetConversationAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<ConversationResponse>>(_ => throw new ForbiddenException("Nope."));
        var sut = new ConversationsController(chat);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => sut.Get(Guid.NewGuid(), CancellationToken.None));
    }
}
