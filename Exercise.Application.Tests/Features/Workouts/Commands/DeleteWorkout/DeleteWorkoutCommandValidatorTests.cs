using Exercise.Application.Features.Workouts.Commands.DeleteWorkout;
using FluentValidation.TestHelper;

public class DeleteWorkoutCommandValidatorTests
{
    private readonly DeleteWorkoutCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidId_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new DeleteWorkoutCommand(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_ShouldHaveErrorWithCustomMessage()
    {
        var result = _validator.TestValidate(new DeleteWorkoutCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Workout Id must not be empty.");
    }
}
