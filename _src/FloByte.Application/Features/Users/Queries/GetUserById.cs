using FloByte.Application.Common.Errors;
using FloByte.Domain.Entities;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Application.Features.Users.Queries;

public record GetUserById(Guid Id) : IQuery<Result<User>>;

public class GetUserByIdHandler : IQueryHandler<GetUserById, Result<User>>
{
    private readonly IApplicationDbContext _context;

    public GetUserByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Result<User>> Handle(GetUserById query, CancellationToken ct)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == query.Id, ct);

        if (user is null)
            return Result.Fail(new NotFoundError($"User with ID {query.Id} not found"));

        return Result.Ok(user);
    }
}
