using FluentValidation;

namespace DocumentService.Application.Commands.UploadDocument;

public sealed class UploadDocumentCommandValidator
    : AbstractValidator<UploadDocumentCommand>
{
    // All MIME types the system accepts
    private static readonly string[] AllowedMimeTypes =
    [
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "text/plain"
    ];

    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required");

        RuleFor(x => x.UploadedByUserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Document title is required")
            .MaximumLength(255)
            .WithMessage("Title cannot exceed 255 characters");

        RuleFor(x => x.MimeType)
            .NotEmpty()
            .WithMessage("File type is required")
            .Must(m => AllowedMimeTypes.Contains(m.ToLower()))
            .WithMessage("File type is not supported. " +
                "Allowed: PDF, Word, Excel, Images, Text");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("File cannot be empty")
            .LessThanOrEqualTo(500L * 1024 * 1024)
            .WithMessage("File size cannot exceed 500MB");
    }
}