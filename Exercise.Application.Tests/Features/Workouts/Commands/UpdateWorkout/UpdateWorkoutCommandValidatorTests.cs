using Exercise.Application.Features.Workouts.Commands.UpdateWorkout;
using FluentValidation.TestHelper;

public class UpdateWorkoutCommandValidatorTests
{
    private readonly UpdateWorkoutCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new UpdateWorkoutCommand
        {
            WorkoutId = Guid.NewGuid(),
            CurrentUserId = Guid.NewGuid(),
            Date      = DateTime.UtcNow
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyWorkoutId_ShouldHaveErrorForWorkoutId()
    {
        var result = _validator.TestValidate(new UpdateWorkoutCommand
        {
            WorkoutId = Guid.Empty,
            CurrentUserId = Guid.NewGuid(),
            Date      = DateTime.UtcNow
        });

        result.ShouldHaveValidationErrorFor(x => x.WorkoutId)
              .WithErrorMessage("WorkoutId is required.");
    }

    [Fact]
    public void Validate_DefaultDate_ShouldHaveErrorForDate()
    {
        var result = _validator.TestValidate(new UpdateWorkoutCommand
        {
            WorkoutId = Guid.NewGuid(),
            CurrentUserId = Guid.NewGuid(),
            Date      = default
        });

        result.ShouldHaveValidationErrorFor(x => x.Date)
              .WithErrorMessage("Date is required.");
    }
}
