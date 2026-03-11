using DocumentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentService.Infrastructure.Persistence.Configurations;

public sealed class DocumentVersionConfiguration
    : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("document_versions");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(v => v.DocumentId)
            .HasColumnName("document_id")
            .IsRequired();

        // Index — fetch all versions for a document fast
        builder.HasIndex(v => v.DocumentId)
            .HasDatabaseName("ix_document_versions_document_id");

        builder.Property(v => v.VersionNumber)
            .HasColumnName("version_number")
            .IsRequired();

        // Unique constraint — document cannot have two version 2s
        builder.HasIndex(v => new { v.DocumentId, v.VersionNumber })
            .IsUnique()
            .HasDatabaseName("ix_document_versions_document_version");

        // StoragePath — owned value object
        builder.OwnsOne(v => v.StoragePath, spBuilder =>
        {
            spBuilder.Property(sp => sp.Value)
                .HasColumnName("storage_path")
                .HasMaxLength(1000)
                .IsRequired();
        });

        // FileSize — owned value object
        builder.OwnsOne(v => v.FileSize, fsBuilder =>
        {
            fsBuilder.Property(fs => fs.Bytes)
                .HasColumnName("file_size_bytes")
                .IsRequired();
        });

        builder.Property(v => v.UploadedByUserId)
            .HasColumnName("uploaded_by_user_id")
            .IsRequired();

        builder.Property(v => v.IsCurrentVersion)
            .HasColumnName("is_current_version")
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // OCR results — nullable until Parser Service processes
        builder.Property(v => v.ExtractedText)
            .HasColumnName("extracted_text")
            .HasColumnType("text"); // Unlimited length for OCR text

        builder.Property(v => v.PageCount)
            .HasColumnName("page_count");
    }
}