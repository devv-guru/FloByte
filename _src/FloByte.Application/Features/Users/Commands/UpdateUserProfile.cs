using FloByte.Application.Common.Errors;
using FloByte.Domain.Entities;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Application.Features.Users.Commands;

public record UpdateUserProfile(
    Guid UserId,
    string FirstName,
    string LastName,
    string? Bio,
    string? AvatarUrl) : ICommand<Result<UserProfile>>;

public class UpdateUserProfileHandler : ICommandHandler<UpdateUserProfile, Result<UserProfile>>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserProfileHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Result<UserProfile>> Handle(UpdateUserProfile command, CancellationToken ct)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == command.UserId, ct);

        if (user is null)
            return Result.Fail(new NotFoundError($"User with ID {command.UserId} not found"));

        user.Profile.UpdatePersonalInfo(command.FirstName, command.LastName, command.Bio);
        
        if (!string.IsNullOrEmpty(command.AvatarUrl))
            user.Profile.UpdateAvatar(command.AvatarUrl);

        await _context.SaveChangesAsync(ct);
        return Result.Ok(user.Profile);
    }
}
