using FloByte.Application.Common.Errors;
using FloByte.Domain.Entities;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Application.Features.Projects.Queries;

public record GetProjectDetails(Guid ProjectId) : IQuery<Result<ProjectDetails>>;

public record ProjectDetails(
    Guid Id,
    string Name,
    string Description,
    List<User> Members,
    List<Workflow> Workflows,
    List<CodeFile> CodeFiles);

public class GetProjectDetailsHandler : IQueryHandler<GetProjectDetails, Result<ProjectDetails>>
{
    private readonly IApplicationDbContext _context;

    public GetProjectDetailsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Result<ProjectDetails>> Handle(GetProjectDetails query, CancellationToken ct)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .Include(p => p.Workflows)
            .Include(p => p.CodeFiles)
            .FirstOrDefaultAsync(p => p.Id == query.ProjectId, ct);

        if (project is null)
            return Result.Fail(new NotFoundError($"Project with ID {query.ProjectId} not found"));

        var details = new ProjectDetails(
            project.Id,
            project.Name,
            project.Description,
            project.Members.ToList(),
            project.Workflows.ToList(),
            project.CodeFiles.ToList());

        return Result.Ok(details);
    }
}
