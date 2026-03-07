using MediatR;
using Shared.Domain.Common;
using DocumentService.Application.DTOs;

namespace DocumentService.Application.Commands.UpdateDocument;

// UpdateDocument — patch-style update
// All fields are optional — only provided fields are updated
// null means "do not change this field"
// empty string means "clear this field"
public record UpdateDocumentCommand(
    Guid DocumentId,
    Guid TenantId,
    Guid RequestedByUserId,
    string? Title = null,
    string? Description = null,
    string? Tags = null) : IRequest<Result<DocumentDto>>;