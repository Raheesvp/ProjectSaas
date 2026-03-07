using FluentValidation;

namespace DocumentService.Application.Commands.UpdateDocument;

public sealed class UpdateDocumentCommandValidator
    : AbstractValidator<UpdateDocumentCommand>
{
    public UpdateDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty()
            .WithMessage("Document ID is required");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required");

        // Title is optional — only validate if provided
        When(x => x.Title is not null, () =>
        {
            RuleFor(x => x.Title!)
                .NotEmpty()
                .WithMessage("Title cannot be empty if provided")
                .MaximumLength(255)
                .WithMessage("Title cannot exceed 255 characters");
        });

        // Tags optional — only validate length if provided
        When(x => x.Tags is not null, () =>
        {
            RuleFor(x => x.Tags!)
                .MaximumLength(500)
                .WithMessage("Tags cannot exceed 500 characters");
        });
    }
}