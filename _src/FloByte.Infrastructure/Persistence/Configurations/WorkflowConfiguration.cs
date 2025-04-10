using FloByte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloByte.Infrastructure.Persistence.Configurations;

public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(w => w.Description)
            .HasMaxLength(1000);

        builder.Property(w => w.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasMany(w => w.Nodes)
            .WithOne(n => n.Workflow)
            .HasForeignKey(n => n.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Connections)
            .WithOne(c => c.Workflow)
            .HasForeignKey(c => c.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(w => !w.Deleted.HasValue);
    }
}

public class WorkflowNodeConfiguration : IEntityTypeConfiguration<WorkflowNode>
{
    public void Configure(EntityTypeBuilder<WorkflowNode> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Configuration)
            .HasColumnType("jsonb");

        builder.HasMany(n => n.OutgoingConnections)
            .WithOne(c => c.SourceNode)
            .HasForeignKey(c => c.SourceNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(n => n.IncomingConnections)
            .WithOne(c => c.TargetNode)
            .HasForeignKey(c => c.TargetNodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkflowConnectionConfiguration : IEntityTypeConfiguration<WorkflowConnection>
{
    public void Configure(EntityTypeBuilder<WorkflowConnection> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Label)
            .HasMaxLength(100);

        builder.Property(c => c.Condition)
            .HasColumnType("jsonb");
    }
}
