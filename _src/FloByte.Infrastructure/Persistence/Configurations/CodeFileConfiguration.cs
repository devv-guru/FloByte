using FloByte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloByte.Infrastructure.Persistence.Configurations;

public class CodeFileConfiguration : IEntityTypeConfiguration<CodeFile>
{
    public void Configure(EntityTypeBuilder<CodeFile> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(f => f.Path)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(f => f.Language)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasMany(f => f.Versions)
            .WithOne(v => v.CodeFile)
            .HasForeignKey(v => v.CodeFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.Comments)
            .WithOne(c => c.CodeFile)
            .HasForeignKey(c => c.CodeFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(f => !f.Deleted.HasValue);
    }
}

public class CodeVersionConfiguration : IEntityTypeConfiguration<CodeVersion>
{
    public void Configure(EntityTypeBuilder<CodeVersion> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Content)
            .IsRequired();

        builder.Property(v => v.CommitMessage)
            .HasMaxLength(500);

        builder.Property(v => v.Hash)
            .HasMaxLength(64)
            .IsRequired();
    }
}

public class CodeCommentConfiguration : IEntityTypeConfiguration<CodeComment>
{
    public void Configure(EntityTypeBuilder<CodeComment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(c => c.LineNumber)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
    }
}
