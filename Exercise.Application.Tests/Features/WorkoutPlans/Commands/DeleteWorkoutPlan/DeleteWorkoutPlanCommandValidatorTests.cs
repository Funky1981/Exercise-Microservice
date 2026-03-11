using Exercise.Application.Features.WorkoutPlans.Commands.DeleteWorkoutPlan;
using FluentValidation.TestHelper;

public class DeleteWorkoutPlanCommandValidatorTests
{
    private readonly DeleteWorkoutPlanCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidWorkoutPlanId_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new DeleteWorkoutPlanCommand
        {
            WorkoutPlanId = Guid.NewGuid(),
            CurrentUserId = Guid.NewGuid()
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyWorkoutPlanId_ShouldHaveErrorWithCustomMessage()
    {
        var result = _validator.TestValidate(new DeleteWorkoutPlanCommand
        {
            WorkoutPlanId = Guid.Empty,
            CurrentUserId = Guid.NewGuid()
        });

        result.ShouldHaveValidationErrorFor(x => x.WorkoutPlanId)
              .WithErrorMessage("WorkoutPlanId is required.");
    }
}
