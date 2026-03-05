using FluentValidation;

namespace Exercise.Application.Features.Users.Commands.UpdateUserProfile
{
    public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
    {
        public UpdateUserProfileCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.HeightCm)
                .GreaterThan(0).WithMessage("Height must be greater than 0.")
                .LessThanOrEqualTo(300).WithMessage("Height must not exceed 300 cm.")
                .When(x => x.HeightCm.HasValue);

            RuleFor(x => x.WeightKg)
                .GreaterThan(0).WithMessage("Weight must be greater than 0.")
                .LessThanOrEqualTo(500).WithMessage("Weight must not exceed 500 kg.")
                .When(x => x.WeightKg.HasValue);

            RuleFor(x => x.UserName)
                .MaximumLength(100).WithMessage("UserName must not exceed 100 characters.")
                .When(x => x.UserName is not null);
        }
    }
}
