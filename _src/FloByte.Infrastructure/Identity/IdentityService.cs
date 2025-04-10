using FloByte.Application.Common.Interfaces;
using FloByte.Domain.Entities;
using FloByte.Domain.ValueObjects;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IApplicationDbContext _context;

    public IdentityService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<User>> GetOrCreateUserAsync(OidcClaims claims)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.SubjectId == claims.SubjectId && u.Provider == claims.Provider);

        if (user is not null)
            return Result.Ok(user);

        user = new User(claims.Email, claims.Name, claims.SubjectId, claims.Provider);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Result.Ok(user);
    }

    public async Task<Result<bool>> IsInRoleAsync(User user, string role)
    {
        var userRoles = await _context.Users
            .Where(u => u.Id == user.Id)
            .Select(u => u.Roles)
            .FirstOrDefaultAsync();

        return Result.Ok(userRoles?.Contains(role) ?? false);
    }

    public async Task<Result> AssignRoleAsync(User user, string role)
    {
        if (await IsInRoleAsync(user, role))
            return Result.Ok();

        user.AddRole(role);
        await _context.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> RemoveRoleAsync(User user, string role)
    {
        if (!await IsInRoleAsync(user, role))
            return Result.Ok();

        user.RemoveRole(role);
        await _context.SaveChangesAsync();

        return Result.Ok();
    }
}
