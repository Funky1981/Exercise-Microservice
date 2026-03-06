using Exercise.Application.Features.ExerciseLogs.Commands.DeleteExerciseLog;
using FluentValidation.TestHelper;

public class DeleteExerciseLogCommandValidatorTests
{
    private readonly DeleteExerciseLogCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidLogId_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new DeleteExerciseLogCommand(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyLogId_ShouldHaveErrorForLogId()
    {
        var result = _validator.TestValidate(new DeleteExerciseLogCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.LogId);
    }
}
