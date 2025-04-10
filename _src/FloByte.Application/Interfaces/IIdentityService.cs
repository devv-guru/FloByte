using FloByte.Domain.Entities;
using FloByte.Domain.ValueObjects;
using FluentResults;

namespace FloByte.Application.Interfaces;

public interface IIdentityService
{
    Task<Result<User>> GetOrCreateUserAsync(OidcClaims claims);
    Task<Result<bool>> IsInRoleAsync(User user, string role);
    Task<Result<bool>> AuthorizeAsync(User user, string policyName);
    Task<Result<string>> GetUserNameAsync(string userId);
}
