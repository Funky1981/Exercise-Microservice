using Exercise.Application.Features.Workouts.Commands.CreateWorkout;
using FluentValidation.TestHelper;

public class CreateWorkoutCommandValidatorTests
{
    private readonly CreateWorkoutCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new CreateWorkoutCommand
        {
            UserId = Guid.NewGuid(),
            Date   = DateTime.UtcNow
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserId_ShouldHaveErrorForUserId()
    {
        var result = _validator.TestValidate(new CreateWorkoutCommand
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
        var result = _validator.TestValidate(new CreateWorkoutCommand
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
        var result = _validator.TestValidate(new CreateWorkoutCommand
        {
            UserId = Guid.NewGuid(),
            Date   = DateTime.UtcNow,
            Name   = new string('X', 201)
        });

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NullName_ShouldHaveNoErrorForName()
    {
        var result = _validator.TestValidate(new CreateWorkoutCommand
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
        var result = _validator.TestValidate(new CreateWorkoutCommand
        {
            UserId = Guid.NewGuid(),
            Date   = DateTime.UtcNow,
            Notes  = new string('N', 1001)
        });

        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }
}
