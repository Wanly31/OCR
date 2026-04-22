using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using OCR.Application.Abstractions;
using OCR.Application.Features.Auth.LoginUser;

namespace OCR.UnitTests.Application.Ocr;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<ITokenService> _tokenRepoMock;
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _tokenRepoMock = new Mock<ITokenService>();

        var store = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        _handler = new LoginUserCommandHandler(
            _userManagerMock.Object,
            _tokenRepoMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_CredentialsAreValid()
    {
        // Arrange
        var user = new IdentityUser { UserName = "test123@gmail.com" };
        var command = new LoginUserCommand("test123@gmail.com", "TestPassword123");
        var roles = new List<string> { "Reader" };

        _userManagerMock
            .Setup(m => m.FindByNameAsync(command.Username))
            .Returns(Task.FromResult<IdentityUser?>(user));

        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, command.Password))
            .Returns(Task.FromResult(true));

        _userManagerMock
            .Setup(m => m.GetRolesAsync(user))
            .Returns(Task.FromResult<IList<string>>(roles));

        _tokenRepoMock
            .Setup(m => m.CreateJWTToken(user, roles))
            .Returns("fake-jwt-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result._jwtToken.Should().Be("fake-jwt-token");
    }
}