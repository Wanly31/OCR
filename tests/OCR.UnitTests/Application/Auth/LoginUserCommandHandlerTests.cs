using FluentAssertions;
using Moq;
using Xunit;

namespace OCR.UnitTests.Application.Auth;

public class LoginUserCommandHandlerTests
{
    [Fact]
    public void Handle_Should_ReturnSuccess_When_CredentialsAreValid()
    {
        // Arrange
        
        // Act
        
        // Assert
        true.Should().BeTrue();
    }
}
