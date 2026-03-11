using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.WorkoutPlans.Commands.UpdateWorkoutPlan;
using Exercise.Application.Tests.TestHelpers;
using Exercise.Domain.Entities;
using FluentAssertions;
using Moq;
using MockFactory = Exercise.Application.Tests.TestHelpers.MockFactory;

public class UpdateWorkoutPlanCommandHandlerTests
{
    private readonly Mock<IWorkoutPlanRepository> _workoutPlanRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateWorkoutPlanCommandHandler _handler;

    public UpdateWorkoutPlanCommandHandlerTests()
    {
        _workoutPlanRepoMock = MockFactory.CreateWorkoutPlanRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        _handler = new UpdateWorkoutPlanCommandHandler(_workoutPlanRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingPlan_ReturnsTrue()
    {
        // Arrange
        var plan = TestDataBuilder.BuildWorkoutPlan();
        _workoutPlanRepoMock
            .Setup(r => r.GetOwnedByIdForUpdateAsync(plan.Id, plan.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        var command = new UpdateWorkoutPlanCommand
        {
            WorkoutPlanId = plan.Id,
            CurrentUserId = plan.UserId,
            Name = "Updated Plan",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            Notes = "Updated notes"
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
            .Setup(r => r.GetOwnedByIdForUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkoutPlan?)null);
        _workoutPlanRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new UpdateWorkoutPlanCommand
        {
            WorkoutPlanId = Guid.NewGuid(),
            CurrentUserId = Guid.NewGuid(),
            Name = "Plan",
            StartDate = DateTime.UtcNow
        };

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exercise.Application.Common.Exceptions.NotFoundException>();
    }
}
