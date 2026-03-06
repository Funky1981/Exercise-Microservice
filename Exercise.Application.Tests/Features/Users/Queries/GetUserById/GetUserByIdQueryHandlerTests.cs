using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.Users.Dtos;
using Exercise.Application.Features.Users.Queries.GetUserById;
using Exercise.Application.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using MockFactory = Exercise.Application.Tests.TestHelpers.MockFactory;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly IMapper _mapper;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userRepoMock = MockFactory.CreateUserRepositoryMock();
        _mapper = MockFactory.CreateMapper();
        _handler = new GetUserByIdQueryHandler(_userRepoMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsMappedDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = TestDataBuilder.BuildUser(id: userId, name: "Alice");

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task Handle_NonExistentUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exercise.Domain.Entities.User?)null);

        // Act
        var result = await _handler.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
