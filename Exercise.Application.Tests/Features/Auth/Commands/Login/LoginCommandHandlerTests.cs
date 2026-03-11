using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Abstractions.Services;
using Exercise.Application.Features.Auth.Commands.Login;
using Exercise.Application.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using MockFactory = Exercise.Application.Tests.TestHelpers.MockFactory;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepoMock     = MockFactory.CreateUserRepositoryMock();
        _tokenServiceMock = MockFactory.CreateTokenServiceMock();
        _unitOfWorkMock   = MockFactory.CreateUnitOfWorkMock();
        _handler = new LoginCommandHandler(_userRepoMock.Object, _tokenServiceMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var user = TestDataBuilder.BuildUser(email: "alice@example.com");
        user.SetPassword("Str0ng!Pass");

        _userRepoMock
            .Setup(r => r.GetByEmailForUpdateAsync("alice@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenServiceMock.Setup(t => t.GenerateToken(user)).Returns("jwt-token");
        _tokenServiceMock.Setup(t => t.GetExpiry()).Returns(DateTime.UtcNow.AddHours(1));
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token-abc");
        _tokenServiceMock.Setup(t => t.GetRefreshTokenExpiry()).Returns(DateTime.UtcNow.AddDays(30));

        var command = new LoginCommand { Email = "alice@example.com", Password = "Str0ng!Pass" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Token.Should().Be("jwt-token");
        result.Email.Should().Be("alice@example.com");
        result.UserId.Should().Be(user.Id);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UnknownEmail_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByEmailForUpdateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exercise.Domain.Entities.User?)null);

        var command = new LoginCommand { Email = "nobody@example.com", Password = "Anything1!" };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = TestDataBuilder.BuildUser(email: "bob@example.com");
        user.SetPassword("Correct1!Pass");

        _userRepoMock
            .Setup(r => r.GetByEmailForUpdateAsync("bob@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new LoginCommand { Email = "bob@example.com", Password = "WrongPass1!" };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
