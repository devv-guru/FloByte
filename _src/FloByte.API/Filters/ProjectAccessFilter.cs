namespace FloByte.API.Filters;

public class ProjectAccessFilter : IEndpointFilter
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ProjectAccessFilter(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var projectId = context.Arguments
            .OfType<Guid>()
            .FirstOrDefault();

        if (projectId == Guid.Empty)
            return await next(context);

        var userId = _currentUser.UserId;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var hasAccess = await _context.Projects
            .AnyAsync(p => p.Id == projectId && 
                (p.Members.Any(m => m.Id.ToString() == userId) || 
                 p.OwnerId.ToString() == userId));

        if (!hasAccess)
            return Results.Forbid();

        return await next(context);
    }
}
