using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.Workouts.Commands.UpdateWorkout;
using Exercise.Application.Tests.TestHelpers;
using Exercise.Domain.Entities;
using FluentAssertions;
using Moq;
using MockFactory = Exercise.Application.Tests.TestHelpers.MockFactory;

public class UpdateWorkoutCommandHandlerTests
{
    private readonly Mock<IExerciseRepository> _exerciseRepoMock;
    private readonly Mock<IWorkoutRepository> _workoutRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateWorkoutCommandHandler _handler;

    public UpdateWorkoutCommandHandlerTests()
    {
        _exerciseRepoMock = MockFactory.CreateExerciseRepositoryMock();
        _workoutRepoMock = MockFactory.CreateWorkoutRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        _handler = new UpdateWorkoutCommandHandler(
            _exerciseRepoMock.Object,
            _workoutRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingWorkout_ReturnsTrue()
    {
        // Arrange
        var workout = TestDataBuilder.BuildWorkout();
        _workoutRepoMock
            .Setup(r => r.GetOwnedByIdWithExercisesForUpdateAsync(workout.Id, workout.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workout);

        var exerciseId = Guid.NewGuid();
        _exerciseRepoMock
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([TestDataBuilder.BuildExercise(id: exerciseId)]);

        var command = new UpdateWorkoutCommand
        {
            WorkoutId = workout.Id,
            CurrentUserId = workout.UserId,
            Name = "Updated Name",
            Date = DateTime.UtcNow.AddDays(1),
            Notes = "Updated notes",
            ExerciseIds = [exerciseId]
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WorkoutNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _workoutRepoMock
            .Setup(r => r.GetOwnedByIdWithExercisesForUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workout?)null);
        _workoutRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new UpdateWorkoutCommand
        {
            WorkoutId = Guid.NewGuid(),
            CurrentUserId = Guid.NewGuid(),
            Name = "Name",
            Date = DateTime.UtcNow
        };

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exercise.Application.Common.Exceptions.NotFoundException>();
    }
}
