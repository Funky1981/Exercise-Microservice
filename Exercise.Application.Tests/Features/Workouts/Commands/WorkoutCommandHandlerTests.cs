using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Models;
using Exercise.Application.Features.Workouts.Commands.AddExerciseToWorkout;
using Exercise.Application.Features.Workouts.Commands.CreateWorkout;
using Exercise.Application.Features.Workouts.Queries.GetWorkoutsByUserId;
using Exercise.Application.Tests.TestHelpers;
using Exercise.Domain.Entities;
using FluentAssertions;
using Moq;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;
using MockFactory = Exercise.Application.Tests.TestHelpers.MockFactory;

public class CreateWorkoutCommandHandlerTests
{
    private readonly Mock<IWorkoutRepository> _workoutRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateWorkoutCommandHandler _handler;

    public CreateWorkoutCommandHandlerTests()
    {
        _workoutRepoMock = MockFactory.CreateWorkoutRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        _handler = new CreateWorkoutCommandHandler(_workoutRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewGuid()
    {
        // Arrange
        var command = new CreateWorkoutCommand
        {
            UserId = Guid.NewGuid(),
            Name   = "Morning Session",
            Date   = DateTime.UtcNow
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsWorkoutAndSaves()
    {
        // Arrange
        var command = new CreateWorkoutCommand
        {
            UserId = Guid.NewGuid(),
            Name   = "Evening Run",
            Date   = DateTime.UtcNow
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _workoutRepoMock.Verify(
            r => r.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class GetWorkoutsByUserIdQueryHandlerTests
{
    private readonly Mock<IWorkoutRepository> _workoutRepoMock;
    private readonly IMapper _mapper;
    private readonly GetWorkoutsByUserIdQueryHandler _handler;

    public GetWorkoutsByUserIdQueryHandlerTests()
    {
        _workoutRepoMock = MockFactory.CreateWorkoutRepositoryMock();
        _mapper = MockFactory.CreateMapper();
        _handler = new GetWorkoutsByUserIdQueryHandler(_workoutRepoMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ExistingWorkouts_ReturnsPagedResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workouts = TestDataBuilder.BuildWorkoutList(userId, count: 5);

        _workoutRepoMock
            .Setup(r => r.GetPagedByUserIdAsync(userId, 0, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync((workouts.Take(3).ToList() as IReadOnlyList<Workout>, 5));

        var query = new GetWorkoutsByUserIdQuery(userId, pageNumber: 1, pageSize: 3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task Handle_EmptyRepository_ReturnsEmptyPagedResult()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _workoutRepoMock
            .Setup(r => r.GetPagedByUserIdAsync(userId, 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Workout>() as IReadOnlyList<Workout>, 0));

        var query = new GetWorkoutsByUserIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}

public class AddExerciseToWorkoutCommandHandlerTests
{
    private readonly Mock<IWorkoutRepository> _workoutRepoMock;
    private readonly Mock<IExerciseRepository> _exerciseRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddExerciseToWorkoutCommandHandler _handler;

    public AddExerciseToWorkoutCommandHandlerTests()
    {
        _workoutRepoMock = MockFactory.CreateWorkoutRepositoryMock();
        _exerciseRepoMock = MockFactory.CreateExerciseRepositoryMock();
        _unitOfWorkMock = MockFactory.CreateUnitOfWorkMock();
        _handler = new AddExerciseToWorkoutCommandHandler(
            _workoutRepoMock.Object, _exerciseRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidIds_AddsExerciseAndSaves()
    {
        // Arrange
        var workout = TestDataBuilder.BuildWorkout();
        var exercise = TestDataBuilder.BuildExercise();

        _workoutRepoMock
            .Setup(r => r.GetByIdWithExercisesAsync(workout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workout);

        _exerciseRepoMock
            .Setup(r => r.GetByIdAsync(exercise.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exercise);

        var command = new AddExerciseToWorkoutCommand(workout.Id, exercise.Id);

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
            .Setup(r => r.GetByIdWithExercisesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workout?)null);

        var command = new AddExerciseToWorkoutCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exercise.Application.Common.Exceptions.NotFoundException>();
    }

    [Fact]
    public async Task Handle_ExerciseNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var workout = TestDataBuilder.BuildWorkout();

        _workoutRepoMock
            .Setup(r => r.GetByIdWithExercisesAsync(workout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workout);

        _exerciseRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExerciseEntity?)null);

        var command = new AddExerciseToWorkoutCommand(workout.Id, Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exercise.Application.Common.Exceptions.NotFoundException>();
    }
}
