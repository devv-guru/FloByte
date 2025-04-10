using FluentValidation;

namespace FloByte.Application.Features.CodeEditor.Commands;

public class SaveCodeFileValidator : AbstractValidator<SaveCodeFile>
{
    private readonly IApplicationDbContext _context;

    public SaveCodeFileValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.FileId)
            .NotEmpty()
            .MustAsync(FileExists)
            .WithMessage("File does not exist");

        RuleFor(x => x.Content)
            .NotNull();

        RuleFor(x => x.CommitMessage)
            .MaximumLength(200)
            .When(x => x.CommitMessage != null);
    }

    private async Task<bool> FileExists(Guid fileId, CancellationToken ct)
    {
        return await _context.CodeFiles
            .AnyAsync(f => f.Id == fileId, ct);
    }
}
