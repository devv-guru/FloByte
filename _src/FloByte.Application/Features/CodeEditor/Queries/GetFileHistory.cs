using FloByte.Application.Common.Errors;
using FloByte.Domain.Entities;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Application.Features.CodeEditor.Queries;

public record GetFileHistory(Guid FileId) : IQuery<Result<List<CodeVersion>>>;

public class GetFileHistoryHandler : IQueryHandler<GetFileHistory, Result<List<CodeVersion>>>
{
    private readonly IApplicationDbContext _context;

    public GetFileHistoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Result<List<CodeVersion>>> Handle(GetFileHistory query, CancellationToken ct)
    {
        var file = await _context.CodeFiles
            .Include(f => f.Versions)
            .ThenInclude(v => v.Author)
            .FirstOrDefaultAsync(f => f.Id == query.FileId, ct);

        if (file is null)
            return Result.Fail(new NotFoundError($"File with ID {query.FileId} not found"));

        return Result.Ok(file.Versions.OrderByDescending(v => v.Created).ToList());
    }
}
