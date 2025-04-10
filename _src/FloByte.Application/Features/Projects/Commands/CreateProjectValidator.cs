using FluentValidation;

namespace FloByte.Application.Features.Projects.Commands;

public class CreateProjectValidator : AbstractValidator<CreateProject>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9\-_\s]+$")
            .WithMessage("Project name can only contain letters, numbers, spaces, hyphens, and underscores");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);
    }
}
