using FloByte.Domain.Common;

namespace FloByte.Domain.Entities;

public class CodeCommentReply : BaseEntity
{
    public CodeComment Comment { get; private set; }
    public User Author { get; private set; }
    public string Content { get; private set; }

    private CodeCommentReply() { } // For EF Core

    public CodeCommentReply(CodeComment comment, User author, string content)
    {
        Id = Guid.NewGuid();
        Comment = comment;
        Author = author;
        Content = content;
    }

    public void UpdateContent(string newContent)
    {
        Content = newContent;
    }
}
