using FloByte.Application.Common.Errors;
using FloByte.Domain.Entities;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Application.Features.Projects.Commands;

public record AddProjectMember(Guid ProjectId, Guid UserId) : ICommand<Result>;

public class AddProjectMemberHandler : ICommandHandler<AddProjectMember, Result>
{
    private readonly IApplicationDbContext _context;

    public AddProjectMemberHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Result> Handle(AddProjectMember command, CancellationToken ct)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, ct);

        if (project is null)
            return Result.Fail(new NotFoundError($"Project with ID {command.ProjectId} not found"));

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId, ct);

        if (user is null)
            return Result.Fail(new NotFoundError($"User with ID {command.UserId} not found"));

        project.AddMember(user);
        await _context.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
