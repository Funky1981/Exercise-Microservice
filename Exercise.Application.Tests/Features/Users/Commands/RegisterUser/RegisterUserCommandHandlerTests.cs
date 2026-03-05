using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.Users.Commands.RegisterUser;
using Exercise.Application.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using MockFactory = Exercise.Application.Tests.TestHelpers.MockFactory;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepoMock = MockFactory.CreateUserRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        _handler = new RegisterUserCommandHandler(_userRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_NewEmail_CreatesUserAndReturnsGuid()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByEmailAsync("new@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exercise.Domain.Entities.User?)null);

        var command = new RegisterUserCommand
        {
            Name     = "Alice",
            Email    = "new@example.com",
            Password = "Str0ng!Pass"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<Exercise.Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var existing = TestDataBuilder.BuildUser(email: "taken@example.com");

        _userRepoMock
            .Setup(r => r.GetByEmailAsync("taken@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var command = new RegisterUserCommand
        {
            Name     = "Bob",
            Email    = "taken@example.com",
            Password = "Str0ng!Pass"
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }
}
