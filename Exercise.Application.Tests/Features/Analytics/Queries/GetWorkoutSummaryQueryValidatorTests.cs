using Exercise.Application.Features.Analytics.Queries.GetWorkoutSummary;
using FluentValidation.TestHelper;

public class GetWorkoutSummaryQueryValidatorTests
{
    private readonly GetWorkoutSummaryQueryValidator _validator = new();

    [Fact]
    public void Validate_ValidUserId_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new GetWorkoutSummaryQuery
        {
            UserId = Guid.NewGuid()
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserId_ShouldHaveErrorForUserId()
    {
        var result = _validator.TestValidate(new GetWorkoutSummaryQuery
        {
            UserId = Guid.Empty
        });

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}
