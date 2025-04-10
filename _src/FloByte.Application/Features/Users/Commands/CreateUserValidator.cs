using FluentValidation;

namespace FloByte.Application.Features.Users.Commands;

public class CreateUserValidator : AbstractValidator<CreateUser>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Claims)
            .NotNull();

        RuleFor(x => x.Claims.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Claims.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Claims.SubjectId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Claims.Provider)
            .NotEmpty()
            .MaximumLength(50);
    }
}
