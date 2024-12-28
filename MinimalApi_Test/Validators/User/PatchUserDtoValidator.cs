using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using MinimalApi_Test.DTOs.User;

namespace MinimalApi_Test.Validators.User
{
    public class PatchUserDtoValidator : AbstractValidator<JsonPatchDocument<PatchUserDto>>
    {
        public PatchUserDtoValidator()
        {
            RuleFor(x => x)
                .NotNull()
                .WithMessage("Patch document cannot be null");

            RuleFor(x => x.Operations)
                .NotEmpty()
                .WithMessage("At least one operation must be specified");

            RuleForEach(x => x.Operations)
                .Must(operation =>
                    new[] { "replace", "add", "remove" }.Contains(operation.op.ToLower()))
                .WithMessage("Only replace, add, and remove operations are allowed")
                .Must(operation =>
                    new[] { "/firstName", "/lastName", "/username", "/role" }
                        .Contains(operation.path.ToLower()))
                .WithMessage("Invalid path specified");
        }
    }
}
