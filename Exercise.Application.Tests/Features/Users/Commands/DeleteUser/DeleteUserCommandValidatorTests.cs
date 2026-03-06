using Exercise.Application.Features.Users.Commands.DeleteUser;
using FluentValidation.TestHelper;

public class DeleteUserCommandValidatorTests
{
    private readonly DeleteUserCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidUserId_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new DeleteUserCommand(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUserId_ShouldHaveErrorForUserId()
    {
        var result = _validator.TestValidate(new DeleteUserCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}
