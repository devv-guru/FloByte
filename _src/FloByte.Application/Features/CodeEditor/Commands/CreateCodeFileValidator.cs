using FluentValidation;
using System.IO;

namespace FloByte.Application.Features.CodeEditor.Commands;

public class CreateCodeFileValidator : AbstractValidator<CreateCodeFile>
{
    private readonly IApplicationDbContext _context;
    private readonly ICodeEditorService _codeEditor;

    public CreateCodeFileValidator(
        IApplicationDbContext context,
        ICodeEditorService codeEditor)
    {
        _context = context;
        _codeEditor = codeEditor;

        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .MustAsync(ProjectExists)
            .WithMessage("Project does not exist");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255)
            .Must(BeValidFileName)
            .WithMessage("Invalid file name");

        RuleFor(x => x.Path)
            .NotEmpty()
            .MaximumLength(1000)
            .Must(BeValidPath)
            .WithMessage("Invalid file path");

        RuleFor(x => x.Language)
            .NotEmpty()
            .MaximumLength(50)
            .MustAsync(BeSupportedLanguage)
            .WithMessage("Unsupported language");

        RuleFor(x => x.Content)
            .NotNull();
    }

    private async Task<bool> ProjectExists(Guid projectId, CancellationToken ct)
    {
        return await _context.Projects
            .AnyAsync(p => p.Id == projectId, ct);
    }

    private static bool BeValidFileName(string fileName)
    {
        return !string.IsNullOrEmpty(fileName) 
            && fileName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
    }

    private static bool BeValidPath(string path)
    {
        return !string.IsNullOrEmpty(path) 
            && path.IndexOfAny(Path.GetInvalidPathChars()) == -1;
    }

    private async Task<bool> BeSupportedLanguage(string language, CancellationToken ct)
    {
        var supportedLanguages = await _codeEditor.GetSupportedLanguagesAsync(ct);
        return supportedLanguages.Contains(language);
    }
}
