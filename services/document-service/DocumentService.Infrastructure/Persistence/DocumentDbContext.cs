using DocumentService.Domain.Entities;
using DocumentService.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace DocumentService.Infrastructure.Persistence;

// DocumentDbContext — EF Core context for Document Service
//
// Design decisions:
// 1. Separate DB context per service — microservice boundary
// 2. PostgreSQL only — not shared with Identity (MS SQL)
// 3. ApplyConfigurationsFromAssembly — auto-discovers all IEntityTypeConfiguration
// 4. DomainEventInterceptor injected — auto-dispatches events after save
// 5. Snake_case naming convention — matches PostgreSQL standards
public sealed class DocumentDbContext : DbContext
{
    public DocumentDbContext(DbContextOptions<DocumentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Auto-discovers DocumentConfiguration + DocumentVersionConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(DocumentDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}