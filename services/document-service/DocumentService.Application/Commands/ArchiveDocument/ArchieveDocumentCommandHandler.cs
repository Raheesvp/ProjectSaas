using DocumentService.Domain.Errors;
using DocumentService.Domain.Repositories;
using MediatR;
using Shared.Domain.Common;

namespace DocumentService.Application.Commands.ArchiveDocument;

public sealed class ArchiveDocumentCommandHandler
    : IRequestHandler<ArchiveDocumentCommand, Result>
{
    private readonly IDocumentRepository _documentRepo;

    public ArchiveDocumentCommandHandler(
        IDocumentRepository documentRepo)
        => _documentRepo = documentRepo;

    public async Task<Result> Handle(
        ArchiveDocumentCommand command,
        CancellationToken cancellationToken)
    {
        // GetByIdAsync takes BOTH ids — enforces tenant isolation
        // A user from Tenant A cannot archive Tenant B's document
        var document = await _documentRepo.GetByIdAsync(
            command.DocumentId,
            command.TenantId,
            cancellationToken);

        if (document is null)
            return Result.Failure(
                DocumentErrors.Document.NotFound(command.DocumentId));

        try
        {
            document.Archive();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(
                new Error("Document.ArchiveFailed", ex.Message));
        }

        await _documentRepo.UpdateAsync(document, cancellationToken);

        return Result.Success();
    }
}