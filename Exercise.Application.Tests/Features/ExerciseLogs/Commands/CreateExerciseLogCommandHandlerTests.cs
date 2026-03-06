using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.ExerciseLogs.Commands.CreateExerciseLog;
using Exercise.Application.Tests.TestHelpers;
using Exercise.Domain.Entities;
using FluentAssertions;
using Moq;
using MockFactory = Exercise.Application.Tests.TestHelpers.MockFactory;

public class CreateExerciseLogCommandHandlerTests
{
    private readonly Mock<IExerciseLogRepository> _exerciseLogRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateExerciseLogCommandHandler _handler;

    public CreateExerciseLogCommandHandlerTests()
    {
        _exerciseLogRepoMock = MockFactory.CreateExerciseLogRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        _handler = new CreateExerciseLogCommandHandler(_exerciseLogRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewGuid()
    {
        // Arrange
        var command = new CreateExerciseLogCommand
        {
            UserId = Guid.NewGuid(),
            Name   = "Monday Log",
            Date   = DateTime.UtcNow
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsLogAndSaves()
    {
        // Arrange
        var command = new CreateExerciseLogCommand
        {
            UserId = Guid.NewGuid(),
            Name   = "Tuesday Log",
            Date   = DateTime.UtcNow
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _exerciseLogRepoMock.Verify(
            r => r.AddAsync(It.IsAny<ExerciseLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
