using FloByte.Domain.Entities;
using FluentResults;

namespace FloByte.Application.Interfaces;

public interface ICodeEditorService
{
    Task<Result<CodeFile>> CreateFileAsync(string name, string path, string language, string content);
    Task<Result<CodeVersion>> SaveFileAsync(CodeFile file, string content, User user, string? commitMessage = null);
    Task<Result<CodeComment>> AddCommentAsync(CodeFile file, User author, string content, int lineNumber);
    Task<Result<CodeCommentReply>> AddCommentReplyAsync(CodeComment comment, User author, string content);
    Task<Result> ResolveCommentAsync(CodeComment comment, User user);
}
