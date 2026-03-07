using Shared.Domain.Primitives;
using DocumentService.Domain.Enums;

namespace DocumentService.Domain.ValueObjects;

// ContentType maps MIME type string to DocumentType enum
// Centralizes the "what type is this file?" logic
// Used during upload to determine processing pipeline
public sealed class ContentType : ValueObject
{
    public string MimeType { get; }
    public DocumentType DocumentType { get; }

    private ContentType(string mimeType, DocumentType documentType)
    {
        MimeType = mimeType;
        DocumentType = documentType;
    }

    //classify the documenttype acccording the documenttype enum 
    public static ContentType Create(string mimeType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mimeType);

        var docType = mimeType.ToLower() switch
        {
            "application/pdf"   => DocumentType.Pdf,
            "application/msword"
            or "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                                => DocumentType.Word,
            "application/vnd.ms-excel"
            or "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                                => DocumentType.Excel,
            var m when m.StartsWith("image/")
                                => DocumentType.Image,
            "text/plain"        => DocumentType.Text,
            _                   => DocumentType.Other
        };

        return new ContentType(mimeType, docType);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MimeType;
    }

    public override string ToString() => MimeType;
}