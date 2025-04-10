using FloByte.Domain.Common;

namespace FloByte.Domain.Entities;

public class CodeVersion : BaseEntity
{
    public CodeFile CodeFile { get; private set; }
    public string Content { get; private set; }
    public User Author { get; private set; }
    public string CommitMessage { get; private set; } = string.Empty;

    private CodeVersion() { } // For EF Core

    public CodeVersion(CodeFile codeFile, string content, User author, string commitMessage = "")
    {
        Id = Guid.NewGuid();
        CodeFile = codeFile;
        Content = content;
        Author = author;
        CommitMessage = commitMessage;
    }

    public void UpdateCommitMessage(string message)
    {
        CommitMessage = message;
    }
}
