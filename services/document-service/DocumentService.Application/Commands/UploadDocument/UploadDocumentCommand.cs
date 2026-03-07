using MediatR;
using Shared.Domain.Common;
using DocumentService.Application.DTOs;

namespace DocumentService.Application.Commands.UploadDocument;

// UploadDocumentCommand — creates new document + stores file
//
// TenantId and UploadedByUserId come from the JWT claims
// They are set by the controller — never trusted from request body
// This prevents a user from uploading documents to another tenant
public record UploadDocumentCommand(
    Guid TenantId,
    Guid UploadedByUserId,
    string Title,
    string MimeType,
    long FileSizeBytes,
    Stream FileContent,
    string? Description = null,
    string? Tags = null) : IRequest<Result<DocumentDto>>;