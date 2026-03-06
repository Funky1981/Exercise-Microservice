using Exercise.Application.Features.Auth.Commands.Login;
using FluentValidation.TestHelper;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new LoginCommand
        {
            Email    = "user@example.com",
            Password = "anypassword"
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyEmail_ShouldHaveErrorForEmail()
    {
        var result = _validator.TestValidate(new LoginCommand
        {
            Email    = string.Empty,
            Password = "anypassword"
        });

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email is required.");
    }

    [Fact]
    public void Validate_InvalidEmailFormat_ShouldHaveErrorForEmail()
    {
        var result = _validator.TestValidate(new LoginCommand
        {
            Email    = "not-an-email",
            Password = "anypassword"
        });

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("A valid email address is required.");
    }

    [Fact]
    public void Validate_EmptyPassword_ShouldHaveErrorForPassword()
    {
        var result = _validator.TestValidate(new LoginCommand
        {
            Email    = "user@example.com",
            Password = string.Empty
        });

        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password is required.");
    }
}
