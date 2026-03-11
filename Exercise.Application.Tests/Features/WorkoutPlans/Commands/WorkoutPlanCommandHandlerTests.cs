using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.WorkoutPlans.Commands.AddWorkoutToWorkoutPlan;
using Exercise.Application.Features.WorkoutPlans.Commands.CreateWorkoutPlan;
using Exercise.Application.Tests.TestHelpers;
using Exercise.Domain.Entities;
using FluentAssertions;
using Moq;
using MockFactory = Exercise.Application.Tests.TestHelpers.MockFactory;

public class CreateWorkoutPlanCommandHandlerTests
{
    private readonly Mock<IWorkoutPlanRepository> _workoutPlanRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateWorkoutPlanCommandHandler _handler;

    public CreateWorkoutPlanCommandHandlerTests()
    {
        _workoutPlanRepoMock = MockFactory.CreateWorkoutPlanRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        _handler = new CreateWorkoutPlanCommandHandler(_workoutPlanRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewGuid()
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand
        {
            UserId    = Guid.NewGuid(),
            Name      = "12-Week Plan",
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _workoutPlanRepoMock.Verify(
            r => r.AddAsync(It.IsAny<WorkoutPlan>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class AddWorkoutToWorkoutPlanCommandHandlerTests
{
    private readonly Mock<IWorkoutPlanRepository> _workoutPlanRepoMock;
    private readonly Mock<IWorkoutRepository> _workoutRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddWorkoutToWorkoutPlanCommandHandler _handler;

    public AddWorkoutToWorkoutPlanCommandHandlerTests()
    {
        _workoutPlanRepoMock = MockFactory.CreateWorkoutPlanRepositoryMock();
        _workoutRepoMock = MockFactory.CreateWorkoutRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        _handler = new AddWorkoutToWorkoutPlanCommandHandler(
            _workoutPlanRepoMock.Object, _workoutRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidIds_AddsWorkoutAndSaves()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plan = TestDataBuilder.BuildWorkoutPlan(userId: userId);
        var workout = TestDataBuilder.BuildWorkout(userId: userId);

        _workoutPlanRepoMock
            .Setup(r => r.GetOwnedByIdWithWorkoutsForUpdateAsync(plan.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        _workoutRepoMock
            .Setup(r => r.GetByIdAsync(workout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workout);

        var command = new AddWorkoutToWorkoutPlanCommand(plan.Id, workout.Id)
        {
            CurrentUserId = userId
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PlanNotFound_ThrowsNotFoundException()
    {
        // Arrange
        _workoutPlanRepoMock
            .Setup(r => r.GetOwnedByIdWithWorkoutsForUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkoutPlan?)null);
        _workoutPlanRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new AddWorkoutToWorkoutPlanCommand(Guid.NewGuid(), Guid.NewGuid())
        {
            CurrentUserId = Guid.NewGuid()
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exercise.Application.Common.Exceptions.NotFoundException>();
    }
}
