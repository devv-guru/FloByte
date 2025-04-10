using FloByte.Application.Common.Errors;
using FloByte.Application.Common.Interfaces;
using FloByte.Domain.Entities;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Application.Features.CodeEditor.Commands;

public record AddCodeComment(
    Guid FileId,
    string Content,
    int LineNumber) : ICommand<Result<CodeComment>>;

public class AddCodeCommentHandler : ICommandHandler<AddCodeComment, Result<CodeComment>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICodeEditorService _codeEditor;
    private readonly ICurrentUserService _currentUser;

    public AddCodeCommentHandler(
        IApplicationDbContext context,
        ICodeEditorService codeEditor,
        ICurrentUserService currentUser)
    {
        _context = context;
        _codeEditor = codeEditor;
        _currentUser = currentUser;
    }

    public async ValueTask<Result<CodeComment>> Handle(AddCodeComment command, CancellationToken ct)
    {
        var file = await _context.CodeFiles
            .FirstOrDefaultAsync(f => f.Id == command.FileId, ct);

        if (file is null)
            return Result.Fail(new NotFoundError($"File with ID {command.FileId} not found"));

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id.ToString() == _currentUser.UserId, ct);

        if (user is null)
            return Result.Fail(new AuthorizationError("User not found"));

        var comment = await _codeEditor.AddCommentAsync(
            file,
            user,
            command.Content,
            command.LineNumber);

        return comment;
    }
}
