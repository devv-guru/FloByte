using FluentValidation;

namespace FloByte.Application.Features.Workflows.Commands;

public class CreateWorkflowValidator : AbstractValidator<CreateWorkflow>
{
    private readonly IApplicationDbContext _context;

    public CreateWorkflowValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .MustAsync(ProjectExists)
            .WithMessage("Project does not exist");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9\-_\s]+$")
            .WithMessage("Workflow name can only contain letters, numbers, spaces, hyphens, and underscores")
            .MustAsync(BeUniqueNameInProject)
            .WithMessage("A workflow with this name already exists in the project");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);
    }

    private async Task<bool> ProjectExists(Guid projectId, CancellationToken ct)
    {
        return await _context.Projects
            .AnyAsync(p => p.Id == projectId, ct);
    }

    private async Task<bool> BeUniqueNameInProject(CreateWorkflow command, string name, CancellationToken ct)
    {
        return !await _context.Workflows
            .AnyAsync(w => w.ProjectId == command.ProjectId && w.Name == name, ct);
    }
}
