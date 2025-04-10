using FloByte.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<Project> Projects { get; }
    DbSet<Workflow> Workflows { get; }
    DbSet<WorkflowNode> WorkflowNodes { get; }
    DbSet<WorkflowConnection> WorkflowConnections { get; }
    DbSet<CodeFile> CodeFiles { get; }
    DbSet<CodeVersion> CodeVersions { get; }
    DbSet<CodeComment> CodeComments { get; }
    DbSet<CodeCommentReply> CodeCommentReplies { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
