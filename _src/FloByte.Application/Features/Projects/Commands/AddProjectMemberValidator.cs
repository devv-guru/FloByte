using FluentValidation;

namespace FloByte.Application.Features.Projects.Commands;

public class AddProjectMemberValidator : AbstractValidator<AddProjectMember>
{
    private readonly IApplicationDbContext _context;

    public AddProjectMemberValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .MustAsync(ProjectExists)
            .WithMessage("Project does not exist");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .MustAsync(UserExists)
            .WithMessage("User does not exist")
            .MustAsync(NotAlreadyMember)
            .WithMessage("User is already a member of this project");
    }

    private async Task<bool> ProjectExists(Guid projectId, CancellationToken ct)
    {
        return await _context.Projects
            .AnyAsync(p => p.Id == projectId, ct);
    }

    private async Task<bool> UserExists(Guid userId, CancellationToken ct)
    {
        return await _context.Users
            .AnyAsync(u => u.Id == userId, ct);
    }

    private async Task<bool> NotAlreadyMember(AddProjectMember command, Guid userId, CancellationToken ct)
    {
        return !await _context.Projects
            .Include(p => p.Members)
            .Where(p => p.Id == command.ProjectId)
            .SelectMany(p => p.Members)
            .AnyAsync(m => m.Id == userId, ct);
    }
}
