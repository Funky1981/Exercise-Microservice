using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.Workouts.Commands.UpdateWorkout;
using Exercise.Application.Tests.TestHelpers;
using Exercise.Domain.Entities;
using FluentAssertions;
using Moq;
using MockFactory = Exercise.Application.Tests.TestHelpers.MockFactory;

public class UpdateWorkoutCommandHandlerTests
{
    private readonly Mock<IWorkoutRepository> _workoutRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateWorkoutCommandHandler _handler;

    public UpdateWorkoutCommandHandlerTests()
    {
        _workoutRepoMock = MockFactory.CreateWorkoutRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        _handler = new UpdateWorkoutCommandHandler(_workoutRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingWorkout_ReturnsTrue()
    {
        // Arrange
        var workout = TestDataBuilder.BuildWorkout();
        _workoutRepoMock
            .Setup(r => r.GetByIdAsync(workout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workout);

        var command = new UpdateWorkoutCommand
        {
            WorkoutId = workout.Id,
            Name = "Updated Name",
            Date = DateTime.UtcNow.AddDays(1),
            Notes = "Updated notes"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _workoutRepoMock.Verify(r => r.UpdateAsync(workout, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WorkoutNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _workoutRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workout?)null);

        var command = new UpdateWorkoutCommand
        {
            WorkoutId = Guid.NewGuid(),
            Name = "Name",
            Date = DateTime.UtcNow
        };

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exercise.Application.Common.Exceptions.NotFoundException>();
    }
}
