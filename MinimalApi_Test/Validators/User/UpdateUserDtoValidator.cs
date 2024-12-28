using FluentValidation;
using MinimalApi_Test.DTOs.User;

namespace MinimalApi_Test.Validators.User
{
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(200).WithMessage("First name must not exceed 200 characters.")
                .When(x => !string.IsNullOrEmpty(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(200).WithMessage("Last name must not exceed 200 characters.")
                .When(x => !string.IsNullOrEmpty(x.LastName));

            RuleFor(x => x.Username)
                .MaximumLength(200).WithMessage("Username must not exceed 200 characters.")
                .When(x => !string.IsNullOrEmpty(x.Username));

            RuleFor(x => x.Password)
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
                .MaximumLength(200).WithMessage("Password must not exceed 200 characters.")
                .When(x => !string.IsNullOrEmpty(x.Password));

            RuleFor(x => x.Role)
                .MaximumLength(200).WithMessage("Role must not exceed 200 characters.")
                .When(x => !string.IsNullOrEmpty(x.Role));
        }
    }
}