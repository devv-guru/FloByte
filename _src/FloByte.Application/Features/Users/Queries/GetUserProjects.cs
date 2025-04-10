using FloByte.Application.Common.Errors;
using FloByte.Domain.Entities;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Application.Features.Users.Queries;

public record GetUserProjects(Guid UserId) : IQuery<Result<List<Project>>>;

public class GetUserProjectsHandler : IQueryHandler<GetUserProjects, Result<List<Project>>>
{
    private readonly IApplicationDbContext _context;

    public GetUserProjectsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Result<List<Project>>> Handle(GetUserProjects query, CancellationToken ct)
    {
        var user = await _context.Users
            .Include(u => u.Projects)
            .FirstOrDefaultAsync(u => u.Id == query.UserId, ct);

        if (user is null)
            return Result.Fail(new NotFoundError($"User with ID {query.UserId} not found"));

        return Result.Ok(user.Projects.ToList());
    }
}
