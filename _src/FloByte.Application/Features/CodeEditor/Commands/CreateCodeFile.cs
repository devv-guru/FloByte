using FloByte.Application.Common.Errors;
using FloByte.Domain.Entities;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Application.Features.CodeEditor.Commands;

public record CreateCodeFile(
    Guid ProjectId,
    string Name,
    string Path,
    string Language,
    string Content) : ICommand<Result<CodeFile>>;

public class CreateCodeFileHandler : ICommandHandler<CreateCodeFile, Result<CodeFile>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICodeEditorService _codeEditor;

    public CreateCodeFileHandler(
        IApplicationDbContext context,
        ICodeEditorService codeEditor)
    {
        _context = context;
        _codeEditor = codeEditor;
    }

    public async ValueTask<Result<CodeFile>> Handle(CreateCodeFile command, CancellationToken ct)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, ct);

        if (project is null)
            return Result.Fail(new NotFoundError($"Project with ID {command.ProjectId} not found"));

        var fileResult = await _codeEditor.CreateFileAsync(
            command.Name,
            command.Path,
            command.Language,
            command.Content);

        if (fileResult.IsFailed)
            return fileResult;

        var file = fileResult.Value;
        project.AddCodeFile(file);
        await _context.SaveChangesAsync(ct);

        return Result.Ok(file);
    }
}
