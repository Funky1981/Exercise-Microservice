using Exercise.Application.Features.WorkoutPlans.Commands.CreateWorkoutPlan;
using FluentValidation.TestHelper;

public class CreateWorkoutPlanCommandValidatorTests
{
    private readonly CreateWorkoutPlanCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new CreateWorkoutPlanCommand
        {
            UserId    = Guid.NewGuid(),
            StartDate = DateTime.UtcNow
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserId_ShouldHaveErrorForUserId()
    {
        var result = _validator.TestValidate(new CreateWorkoutPlanCommand
        {
            UserId    = Guid.Empty,
            StartDate = DateTime.UtcNow
        });

        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage("UserId is required.");
    }

    [Fact]
    public void Validate_DefaultStartDate_ShouldHaveErrorForStartDate()
    {
        var result = _validator.TestValidate(new CreateWorkoutPlanCommand
        {
            UserId    = Guid.NewGuid(),
            StartDate = default
        });

        result.ShouldHaveValidationErrorFor(x => x.StartDate)
              .WithErrorMessage("StartDate is required.");
    }

    [Fact]
    public void Validate_EndDateBeforeStartDate_ShouldHaveErrorForEndDate()
    {
        var startDate = DateTime.UtcNow;

        var result = _validator.TestValidate(new CreateWorkoutPlanCommand
        {
            UserId    = Guid.NewGuid(),
            StartDate = startDate,
            EndDate   = startDate.AddDays(-1)
        });

        result.ShouldHaveValidationErrorFor(x => x.EndDate)
              .WithErrorMessage("EndDate must be after StartDate.");
    }

    [Fact]
    public void Validate_EndDateAfterStartDate_ShouldHaveNoErrorForEndDate()
    {
        var startDate = DateTime.UtcNow;

        var result = _validator.TestValidate(new CreateWorkoutPlanCommand
        {
            UserId    = Guid.NewGuid(),
            StartDate = startDate,
            EndDate   = startDate.AddDays(30)
        });

        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Validate_NullEndDate_ShouldHaveNoErrorForEndDate()
    {
        var result = _validator.TestValidate(new CreateWorkoutPlanCommand
        {
            UserId    = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate   = null
        });

        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Validate_NameExceeds200Chars_ShouldHaveErrorForName()
    {
        var result = _validator.TestValidate(new CreateWorkoutPlanCommand
        {
            UserId    = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            Name      = new string('N', 201)
        });

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
