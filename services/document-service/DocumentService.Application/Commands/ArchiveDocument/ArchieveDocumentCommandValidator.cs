using FluentValidation;

namespace DocumentService.Application.Commands.ArchiveDocument;

public sealed class ArchiveDocumentCommandValidator
    : AbstractValidator<ArchiveDocumentCommand>
{
    public ArchiveDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage("Document ID is required");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required");

        RuleFor(x => x.RequestedByUserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}