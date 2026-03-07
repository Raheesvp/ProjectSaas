using Shared.Domain.Primitives;
using DocumentService.Domain.ValueObjects;

namespace DocumentService.Domain.Entities;

// DocumentVersion is a child entity of Document aggregate
// Never accessed directly — always through Document.AddVersion()
// Tracks full history of every file uploaded to a document
//
// Real world: Google Docs, SharePoint, Confluence all use this pattern
// Every save creates a new version — previous versions never deleted
public sealed class DocumentVersion : BaseEntity<Guid>
{
    private DocumentVersion() { } // EF Core

    internal DocumentVersion(
        Guid documentId,
        int versionNumber,
        StoragePath storagePath,
        FileSize fileSize,
        string uploadedByUserId) : base(Guid.NewGuid())
    {
        DocumentId    = documentId;
        VersionNumber = versionNumber;
        StoragePath   = storagePath;
        FileSize      = fileSize;
        UploadedByUserId = uploadedByUserId;
        CreatedAt     = DateTime.UtcNow;
        IsCurrentVersion = false; // Set by Document aggregate
    }

    public Guid   DocumentId       { get; private set; }
    public int    VersionNumber    { get; private set; }
    public StoragePath StoragePath { get; private set; } = null!;
    public FileSize    FileSize    { get; private set; } = null!;
    public string UploadedByUserId { get; private set; } = string.Empty;
    public DateTime CreatedAt      { get; private set; }
    public bool   IsCurrentVersion { get; internal set; }

    // OCR results stored on the version — not the document
    // Each version can have different extracted text
    //first one has blurry image then next one has the quality image 
    public string? ExtractedText   { get; private set; }
    public int?   PageCount        { get; private set; }

    internal void SetOcrResult(string extractedText, int pageCount)
    {
        ExtractedText = extractedText;
        PageCount     = pageCount;
    }
}