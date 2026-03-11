using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.Analytics.Queries.GetWorkoutSummary;
using Exercise.Application.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using MockFactory = Exercise.Application.Tests.TestHelpers.MockFactory;

public class GetWorkoutSummaryQueryHandlerTests
{
    private readonly Mock<IWorkoutRepository> _workoutRepoMock;
    private readonly Mock<IExerciseLogRepository> _exerciseLogRepoMock;
    private readonly GetWorkoutSummaryQueryHandler _handler;

    public GetWorkoutSummaryQueryHandlerTests()
    {
        _workoutRepoMock = MockFactory.CreateWorkoutRepositoryMock();
        _exerciseLogRepoMock = MockFactory.CreateExerciseLogRepositoryMock();
        _handler = new GetWorkoutSummaryQueryHandler(_workoutRepoMock.Object, _exerciseLogRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WithWorkoutsAndLogs_ReturnsSummary()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _workoutRepoMock
            .Setup(r => r.GetSummaryByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((3, 0, TimeSpan.Zero));

        _exerciseLogRepoMock
            .Setup(r => r.GetSummaryByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((2, 0, TimeSpan.Zero));

        var query = new GetWorkoutSummaryQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.TotalWorkouts.Should().Be(3);
        result.CompletedWorkouts.Should().Be(0); // none completed in builder
        result.TotalExerciseLogs.Should().Be(2);
        result.CompletedExerciseLogs.Should().Be(0);
        result.TotalWorkoutDuration.Should().Be(TimeSpan.Zero);
        result.TotalExerciseLogDuration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task Handle_NoWorkoutsOrLogs_ReturnsZeroSummary()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _workoutRepoMock
            .Setup(r => r.GetSummaryByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0, TimeSpan.Zero));

        _exerciseLogRepoMock
            .Setup(r => r.GetSummaryByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0, TimeSpan.Zero));

        var query = new GetWorkoutSummaryQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalWorkouts.Should().Be(0);
        result.TotalExerciseLogs.Should().Be(0);
        result.TotalWorkoutDuration.Should().Be(TimeSpan.Zero);
    }
}
