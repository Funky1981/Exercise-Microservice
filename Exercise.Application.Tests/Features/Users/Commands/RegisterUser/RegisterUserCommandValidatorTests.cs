using Exercise.Application.Features.Users.Commands.RegisterUser;
using FluentValidation.TestHelper;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldHaveNoErrors()
    {
        var result = _validator.TestValidate(new RegisterUserCommand
        {
            Name     = "Test User",
            Email    = "test@example.com",
            Password = "Test@Password1"
        });

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_ShouldHaveErrorForName()
    {
        var result = _validator.TestValidate(new RegisterUserCommand
        {
            Name     = string.Empty,
            Email    = "test@example.com",
            Password = "Test@Password1"
        });

        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Validate_NameExceeds200Chars_ShouldHaveErrorForName()
    {
        var result = _validator.TestValidate(new RegisterUserCommand
        {
            Name     = new string('A', 201),
            Email    = "test@example.com",
            Password = "Test@Password1"
        });

        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must not exceed 200 characters.");
    }

    [Fact]
    public void Validate_EmptyEmail_ShouldHaveErrorForEmail()
    {
        var result = _validator.TestValidate(new RegisterUserCommand
        {
            Name     = "Test User",
            Email    = string.Empty,
            Password = "Test@Password1"
        });

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email is required.");
    }

    [Fact]
    public void Validate_InvalidEmailFormat_ShouldHaveErrorForEmail()
    {
        var result = _validator.TestValidate(new RegisterUserCommand
        {
            Name     = "Test User",
            Email    = "not-an-email",
            Password = "Test@Password1"
        });

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("A valid email address is required.");
    }

    [Fact]
    public void Validate_EmailExceeds256Chars_ShouldHaveErrorForEmail()
    {
        var result = _validator.TestValidate(new RegisterUserCommand
        {
            Name     = "Test User",
            Email    = new string('a', 251) + "@b.com",
            Password = "Test@Password1"
        });

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email must not exceed 256 characters.");
    }

    [Fact]
    public void Validate_EmptyPassword_ShouldHaveErrorForPassword()
    {
        var result = _validator.TestValidate(new RegisterUserCommand
        {
            Name     = "Test User",
            Email    = "test@example.com",
            Password = string.Empty
        });

        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password is required.");
    }

    [Fact]
    public void Validate_PasswordTooShort_ShouldHaveErrorForPassword()
    {
        var result = _validator.TestValidate(new RegisterUserCommand
        {
            Name     = "Test User",
            Email    = "test@example.com",
            Password = "Sh@1"
        });

        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must be at least 8 characters.");
    }

    [Fact]
    public void Validate_PasswordNoUppercase_ShouldHaveErrorForPassword()
    {
        var result = _validator.TestValidate(new RegisterUserCommand
        {
            Name     = "Test User",
            Email    = "test@example.com",
            Password = "test@password1"
        });

        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void Validate_PasswordNoLowercase_ShouldHaveErrorForPassword()
    {
        var result = _validator.TestValidate(new RegisterUserCommand
        {
            Name     = "Test User",
            Email    = "test@example.com",
            Password = "TEST@PASSWORD1"
        });

        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public void Validate_PasswordNoDigit_ShouldHaveErrorForPassword()
    {
        var result = _validator.TestValidate(new RegisterUserCommand
        {
            Name     = "Test User",
            Email    = "test@example.com",
            Password = "Test@Password"
        });

        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must contain at least one digit.");
    }

    [Fact]
    public void Validate_PasswordNoSpecialChar_ShouldHaveErrorForPassword()
    {
        var result = _validator.TestValidate(new RegisterUserCommand
        {
            Name     = "Test User",
            Email    = "test@example.com",
            Password = "TestPassword1"
        });

        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must contain at least one special character.");
    }

    [Fact]
    public void Validate_UserNameExceeds100Chars_ShouldHaveErrorForUserName()
    {
        var result = _validator.TestValidate(new RegisterUserCommand
        {
            Name     = "Test User",
            Email    = "test@example.com",
            Password = "Test@Password1",
            UserName = new string('u', 101)
        });

        result.ShouldHaveValidationErrorFor(x => x.UserName)
              .WithErrorMessage("UserName must not exceed 100 characters.");
    }

    [Fact]
    public void Validate_NullUserName_ShouldHaveNoErrorForUserName()
    {
        var result = _validator.TestValidate(new RegisterUserCommand
        {
            Name     = "Test User",
            Email    = "test@example.com",
            Password = "Test@Password1",
            UserName = null
        });

        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }
}
