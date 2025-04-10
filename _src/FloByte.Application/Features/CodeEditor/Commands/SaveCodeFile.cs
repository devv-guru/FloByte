using FloByte.Application.Common.Errors;
using FloByte.Application.Common.Interfaces;
using FloByte.Domain.Entities;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Application.Features.CodeEditor.Commands;

public record SaveCodeFile(
    Guid FileId,
    string Content,
    string? CommitMessage = null) : ICommand<Result<CodeVersion>>;

public class SaveCodeFileHandler : ICommandHandler<SaveCodeFile, Result<CodeVersion>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICodeEditorService _codeEditor;
    private readonly ICurrentUserService _currentUser;

    public SaveCodeFileHandler(
        IApplicationDbContext context,
        ICodeEditorService codeEditor,
        ICurrentUserService currentUser)
    {
        _context = context;
        _codeEditor = codeEditor;
        _currentUser = currentUser;
    }

    public async ValueTask<Result<CodeVersion>> Handle(SaveCodeFile command, CancellationToken ct)
    {
        var file = await _context.CodeFiles
            .FirstOrDefaultAsync(f => f.Id == command.FileId, ct);

        if (file is null)
            return Result.Fail(new NotFoundError($"File with ID {command.FileId} not found"));

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id.ToString() == _currentUser.UserId, ct);

        if (user is null)
            return Result.Fail(new AuthorizationError("User not found"));

        var version = await _codeEditor.SaveFileAsync(
            file,
            command.Content,
            user,
            command.CommitMessage);

        return version;
    }
}
