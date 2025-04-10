using FloByte.Domain.Common;

namespace FloByte.Domain.Entities;

public class CodeFile : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Path { get; private set; } = string.Empty;
    public string Language { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    private readonly List<CodeVersion> _versions = new();
    private readonly List<CodeComment> _comments = new();
    
    public IReadOnlyCollection<CodeVersion> Versions => _versions.AsReadOnly();
    public IReadOnlyCollection<CodeComment> Comments => _comments.AsReadOnly();

    private CodeFile() { } // For EF Core

    public CodeFile(string name, string path, string language, string content)
    {
        Id = Guid.NewGuid();
        Name = name;
        Path = path;
        Language = language;
        Content = content;
    }

    public void UpdateContent(string newContent, User user)
    {
        var version = new CodeVersion(this, Content, user);
        _versions.Add(version);
        Content = newContent;
    }

    public void AddComment(CodeComment comment)
    {
        if (!_comments.Contains(comment))
        {
            _comments.Add(comment);
        }
    }

    public void UpdatePath(string newPath)
    {
        Path = newPath;
    }
}
