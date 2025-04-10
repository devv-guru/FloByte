using FloByte.Domain.Entities;
using FloByte.Domain.ValueObjects;
using FluentResults;
using Mediator;

namespace FloByte.Application.Features.Users.Commands;

public record CreateUser(OidcClaims Claims) : ICommand<Result<User>>;

public class CreateUserHandler : ICommandHandler<CreateUser, Result<User>>
{
    private readonly IApplicationDbContext _context;

    public CreateUserHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Result<User>> Handle(CreateUser command, CancellationToken ct)
    {
        var user = new User(
            command.Claims.Email,
            command.Claims.Name,
            command.Claims.SubjectId,
            command.Claims.Provider);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        return Result.Ok(user);
    }
}
