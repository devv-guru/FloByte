using FloByte.Application.Common.Interfaces;
using FloByte.Domain.Entities;
using FluentResults;
using Mediator;

namespace FloByte.Application.Features.Projects.Commands;

public record CreateProject(string Name, string Description) : ICommand<Result<Project>>;

public class CreateProjectHandler : ICommandHandler<CreateProject, Result<Project>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateProjectHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Result<Project>> Handle(CreateProject command, CancellationToken ct)
    {
        var project = new Project(command.Name, command.Description);
        
        _context.Projects.Add(project);
        await _context.SaveChangesAsync(ct);

        return Result.Ok(project);
    }
}
