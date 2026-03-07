using MediatR;
using Shared.Domain.Common;

namespace DocumentService.Application.Commands.ArchiveDocument;

// ArchiveDocument — soft delete
// Document is hidden from all list views
// File remains in MinIO — never physically deleted
// Audit trail preserved — compliance requirement
public record ArchiveDocumentCommand(
    Guid DocumentId,
    Guid TenantId,
    Guid RequestedByUserId) : IRequest<Result>;