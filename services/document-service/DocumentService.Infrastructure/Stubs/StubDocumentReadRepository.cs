using DocumentService.Domain.Repositories;

namespace DocumentService.Infrastructure.Stubs;

public sealed class StubDocumentReadRepository : IDocumentReadRepository
{
    public Task<DocumentSummary?> GetSummaryByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => Task.FromResult<DocumentSummary?>(null);

    public Task<PagedResult<DocumentSummary>> GetPagedAsync(Guid tenantId, DocumentQueryFilter filter, CancellationToken ct = default)
        => Task.FromResult(new PagedResult<DocumentSummary>(
            new List<DocumentSummary>().AsReadOnly(), 0, 1, 20));

    public Task<IReadOnlyList<DocumentVersionSummary>> GetVersionsAsync(Guid documentId, Guid tenantId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<DocumentVersionSummary>>(
            new List<DocumentVersionSummary>().AsReadOnly());
}
