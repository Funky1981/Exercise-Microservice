using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.ExerciseLogs.Commands.DeleteExerciseLog;
using Exercise.Application.Tests.TestHelpers;
using Exercise.Domain.Entities;
using FluentAssertions;
using Moq;
using MockFactory = Exercise.Application.Tests.TestHelpers.MockFactory;

public class DeleteExerciseLogCommandHandlerTests
{
    private readonly Mock<IExerciseLogRepository> _exerciseLogRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteExerciseLogCommandHandler _handler;

    public DeleteExerciseLogCommandHandlerTests()
    {
        _exerciseLogRepoMock = MockFactory.CreateExerciseLogRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        _handler = new DeleteExerciseLogCommandHandler(_exerciseLogRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingLog_DeletesAndSaves()
    {
        // Arrange
        var log = TestDataBuilder.BuildExerciseLog();
        var currentUserId = log.UserId;
        _exerciseLogRepoMock
            .Setup(r => r.GetOwnedByIdForUpdateAsync(log.Id, currentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(log);

        var command = new DeleteExerciseLogCommand(log.Id) { CurrentUserId = currentUserId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _exerciseLogRepoMock.Verify(r => r.DeleteAsync(log, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_LogNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _exerciseLogRepoMock
            .Setup(r => r.GetOwnedByIdForUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExerciseLog?)null);
        _exerciseLogRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new DeleteExerciseLogCommand(Guid.NewGuid()) { CurrentUserId = Guid.NewGuid() };

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exercise.Application.Common.Exceptions.NotFoundException>();
    }
}
