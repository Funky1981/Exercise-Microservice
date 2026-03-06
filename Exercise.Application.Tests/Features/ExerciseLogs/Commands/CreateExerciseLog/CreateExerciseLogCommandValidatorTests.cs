using Exercise.Application.Features.ExerciseLogs.Commands.CreateExerciseLog;
using FluentValidation.TestHelper;

public class CreateExerciseLogCommandValidatorTests
{
    private readonly CreateExerciseLogCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new CreateExerciseLogCommand
        {
            UserId = Guid.NewGuid(),
            Date   = DateTime.UtcNow
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserId_ShouldHaveErrorForUserId()
    {
        var result = _validator.TestValidate(new CreateExerciseLogCommand
        {
            UserId = Guid.Empty,
            Date   = DateTime.UtcNow
        });

        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage("UserId is required.");
    }

    [Fact]
    public void Validate_DefaultDate_ShouldHaveErrorForDate()
    {
        var result = _validator.TestValidate(new CreateExerciseLogCommand
        {
            UserId = Guid.NewGuid(),
            Date   = default
        });

        result.ShouldHaveValidationErrorFor(x => x.Date)
              .WithErrorMessage("Date is required.");
    }

    [Fact]
    public void Validate_NameExceeds200Chars_ShouldHaveErrorForName()
    {
        var result = _validator.TestValidate(new CreateExerciseLogCommand
        {
            UserId = Guid.NewGuid(),
            Date   = DateTime.UtcNow,
            Name   = new string('X', 201)
        });

        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must not exceed 200 characters.");
    }

    [Fact]
    public void Validate_NullName_ShouldHaveNoErrorForName()
    {
        var result = _validator.TestValidate(new CreateExerciseLogCommand
        {
            UserId = Guid.NewGuid(),
            Date   = DateTime.UtcNow,
            Name   = null
        });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NotesExceeds1000Chars_ShouldHaveErrorForNotes()
    {
        var result = _validator.TestValidate(new CreateExerciseLogCommand
        {
            UserId = Guid.NewGuid(),
            Date   = DateTime.UtcNow,
            Notes  = new string('N', 1001)
        });

        result.ShouldHaveValidationErrorFor(x => x.Notes)
              .WithErrorMessage("Notes must not exceed 1000 characters.");
    }
}
