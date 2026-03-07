using DocumentService.Application.DTOs;
using DocumentService.Domain.Errors;
using DocumentService.Domain.Repositories;
using DocumentService.Domain.ValueObjects;
using MediatR;
using Shared.Domain.Common;

namespace DocumentService.Application.Commands.UpdateDocument;

public sealed class UpdateDocumentCommandHandler
    : IRequestHandler<UpdateDocumentCommand, Result<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepo;

    public UpdateDocumentCommandHandler(
        IDocumentRepository documentRepo)
        => _documentRepo = documentRepo;

    public async Task<Result<DocumentDto>> Handle(
        UpdateDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var document = await _documentRepo.GetByIdAsync(
            command.DocumentId,
            command.TenantId,
            cancellationToken);

        if (document is null)
            return Result.Failure<DocumentDto>(
                DocumentErrors.Document.NotFound(command.DocumentId));

        if (document.Status ==
            Domain.Enums.DocumentStatus.Archived)
            return Result.Failure<DocumentDto>(
                DocumentErrors.Document.Archived);

        // Only update fields that were provided
        // null = keep existing value
        if (command.Title is not null)
        {
            try
            {
                document.UpdateTitle(
                    DocumentTitle.Create(command.Title));
            }
            catch (ArgumentException ex)
            {
                return Result.Failure<DocumentDto>(
                    new Error("Document.InvalidTitle", ex.Message));
            }
        }

        if (command.Description is not null)
            document.UpdateDescription(command.Description);

        if (command.Tags is not null)
            document.UpdateTags(command.Tags);

        await _documentRepo.UpdateAsync(document, cancellationToken);

        var current = document.CurrentVersion!;
        return Result.Success(new DocumentDto(
            document.Id,
            document.TenantId,
            document.Title.Value,
            document.Status.ToString(),
            document.ContentType.DocumentType.ToString(),
            document.ContentType.MimeType,
            current.FileSize.Bytes,
            current.FileSize.ToString(),
            document.Versions.Count,
            current.VersionNumber,
            current.StoragePath.Value,
            document.UploadedByUserId.ToString(),
            document.CreatedAt,
            document.UpdatedAt,
            document.Description,
            document.Tags));
    }
}