using FloByte.Domain.Common;

namespace FloByte.Domain.Entities;

public class CodeComment : BaseEntity
{
    public CodeFile CodeFile { get; private set; }
    public User Author { get; private set; }
    public string Content { get; private set; }
    public int LineNumber { get; private set; }
    public bool IsResolved { get; private set; }
    private readonly List<CodeCommentReply> _replies = new();
    
    public IReadOnlyCollection<CodeCommentReply> Replies => _replies.AsReadOnly();

    private CodeComment() { } // For EF Core

    public CodeComment(CodeFile codeFile, User author, string content, int lineNumber)
    {
        Id = Guid.NewGuid();
        CodeFile = codeFile;
        Author = author;
        Content = content;
        LineNumber = lineNumber;
        IsResolved = false;
    }

    public void AddReply(CodeCommentReply reply)
    {
        if (!_replies.Contains(reply))
        {
            _replies.Add(reply);
        }
    }

    public void UpdateContent(string newContent)
    {
        Content = newContent;
    }

    public void Resolve()
    {
        IsResolved = true;
    }

    public void Reopen()
    {
        IsResolved = false;
    }
}
