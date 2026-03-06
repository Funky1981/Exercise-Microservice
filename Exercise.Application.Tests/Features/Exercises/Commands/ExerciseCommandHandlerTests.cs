using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using Exercise.Application.Features.Exercises.Commands.CreateExercise;
using Exercise.Application.Features.Exercises.Commands.DeleteExercise;
using Exercise.Application.Features.Exercises.Commands.UpdateExercise;
using FluentAssertions;
using Moq;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

// ── CreateExerciseCommandHandler ─────────────────────────────────────────────

public class CreateExerciseCommandHandlerTests
{
    private readonly Mock<IExerciseRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork>         _uowMock  = new();
    private readonly CreateExerciseCommandHandler _handler;

    public CreateExerciseCommandHandlerTests()
        => _handler = new CreateExerciseCommandHandler(_repoMock.Object, _uowMock.Object);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewGuid()
    {
        var command = new CreateExerciseCommand
        {
            Name         = "Push Up",
            BodyPart     = "Chest",
            TargetMuscle = "Pectorals"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsExerciseAndSaves()
    {
        var command = new CreateExerciseCommand
        {
            Name         = "Squat",
            BodyPart     = "Legs",
            TargetMuscle = "Quadriceps",
            Equipment    = "Barbell",
            Difficulty   = "Hard"
        };

        await _handler.Handle(command, CancellationToken.None);

        _repoMock.Verify(
            r => r.AddAsync(It.IsAny<ExerciseEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

// ── UpdateExerciseCommandHandler ─────────────────────────────────────────────

public class UpdateExerciseCommandHandlerTests
{
    private readonly Mock<IExerciseRepository>  _repoMock = new();
    private readonly Mock<IUnitOfWork>           _uowMock  = new();
    private readonly UpdateExerciseCommandHandler _handler;

    private static ExerciseEntity MakeExercise(Guid id)
        => new(id, "Push Up", "Chest", "Pectorals");

    public UpdateExerciseCommandHandlerTests()
        => _handler = new UpdateExerciseCommandHandler(_repoMock.Object, _uowMock.Object);

    [Fact]
    public async Task Handle_ExerciseExists_UpdatesAndSaves()
    {
        var id       = Guid.NewGuid();
        var exercise = MakeExercise(id);

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(exercise);

        var command = new UpdateExerciseCommand
        {
            Id           = id,
            Name         = "Diamond Push Up",
            BodyPart     = "Chest",
            TargetMuscle = "Triceps",
            Difficulty   = "Hard"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _repoMock.Verify(r => r.UpdateAsync(exercise, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExerciseNotFound_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ExerciseEntity?)null);

        var act = () => _handler.Handle(
            new UpdateExerciseCommand { Id = Guid.NewGuid(), Name = "X", BodyPart = "Y", TargetMuscle = "Z" },
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

// ── DeleteExerciseCommandHandler ─────────────────────────────────────────────

public class DeleteExerciseCommandHandlerTests
{
    private readonly Mock<IExerciseRepository>  _repoMock = new();
    private readonly Mock<IUnitOfWork>           _uowMock  = new();
    private readonly DeleteExerciseCommandHandler _handler;

    private static ExerciseEntity MakeExercise(Guid id)
        => new(id, "Push Up", "Chest", "Pectorals");

    public DeleteExerciseCommandHandlerTests()
        => _handler = new DeleteExerciseCommandHandler(_repoMock.Object, _uowMock.Object);

    [Fact]
    public async Task Handle_ExerciseExists_DeletesAndSaves()
    {
        var id       = Guid.NewGuid();
        var exercise = MakeExercise(id);

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(exercise);

        var result = await _handler.Handle(new DeleteExerciseCommand(id), CancellationToken.None);

        result.Should().BeTrue();
        _repoMock.Verify(r => r.DeleteAsync(exercise, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExerciseNotFound_ThrowsNotFoundException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ExerciseEntity?)null);

        var act = () => _handler.Handle(
            new DeleteExerciseCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
