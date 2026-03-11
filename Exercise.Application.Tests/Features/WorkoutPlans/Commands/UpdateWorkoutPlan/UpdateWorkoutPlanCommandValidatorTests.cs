using Exercise.Application.Features.WorkoutPlans.Commands.UpdateWorkoutPlan;
using FluentValidation.TestHelper;

public class UpdateWorkoutPlanCommandValidatorTests
{
    private readonly UpdateWorkoutPlanCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new UpdateWorkoutPlanCommand
        {
            WorkoutPlanId = Guid.NewGuid(),
            CurrentUserId = Guid.NewGuid(),
            StartDate     = DateTime.UtcNow
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyWorkoutPlanId_ShouldHaveErrorForWorkoutPlanId()
    {
        var result = _validator.TestValidate(new UpdateWorkoutPlanCommand
        {
            WorkoutPlanId = Guid.Empty,
            CurrentUserId = Guid.NewGuid(),
            StartDate     = DateTime.UtcNow
        });

        result.ShouldHaveValidationErrorFor(x => x.WorkoutPlanId)
              .WithErrorMessage("WorkoutPlanId is required.");
    }

    [Fact]
    public void Validate_DefaultStartDate_ShouldHaveErrorForStartDate()
    {
        var result = _validator.TestValidate(new UpdateWorkoutPlanCommand
        {
            WorkoutPlanId = Guid.NewGuid(),
            CurrentUserId = Guid.NewGuid(),
            StartDate     = default
        });

        result.ShouldHaveValidationErrorFor(x => x.StartDate)
              .WithErrorMessage("StartDate is required.");
    }
}
