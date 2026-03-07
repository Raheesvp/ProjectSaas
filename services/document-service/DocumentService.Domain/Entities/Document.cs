using Shared.Domain.Primitives;
using DocumentService.Domain.Enums;
using DocumentService.Domain.Events;
using DocumentService.Domain.ValueObjects;

namespace DocumentService.Domain.Entities;

// Document is the central Aggregate Root of this entire service
// All document operations flow through this class
//
// Key design decisions:
// 1. Status transitions are enforced — cannot jump from Uploading to Approved
// 2. Versions are a private list — only AddVersion() can add
// 3. Factory method Create() is the only way to instantiate
// 4. Domain events raised for every significant state change
// 5. TenantId on aggregate ensures all queries are tenant-scoped
public sealed class Document : AggregateRoot<Guid>
{
    private readonly List<DocumentVersion> _versions = new();

    private Document() { } // EF Core

    private Document(
        Guid id,
        Guid tenantId,
        Guid uploadedByUserId,
        DocumentTitle title,
        ContentType contentType,
        StoragePath storagePath,
        FileSize fileSize) : base(id)
    {
        TenantId        = tenantId;
        UploadedByUserId = uploadedByUserId;
        Title           = title;
        ContentType     = contentType;
        Status          = DocumentStatus.Uploading;
        CreatedAt       = DateTime.UtcNow;
        UpdatedAt       = DateTime.UtcNow;

        // First version is created with the document
        var firstVersion = new DocumentVersion(
            id, 1, storagePath, fileSize, uploadedByUserId.ToString());
        firstVersion.IsCurrentVersion = true;
        _versions.Add(firstVersion);
    }

    public Guid          TenantId          { get; private set; }
    public Guid          UploadedByUserId  { get; private set; }
    public DocumentTitle Title             { get; private set; } = null!;
    public ContentType   ContentType       { get; private set; } = null!;
    public DocumentStatus Status           { get; private set; }
    public DateTime      CreatedAt         { get; private set; }
    public DateTime      UpdatedAt         { get; private set; }
    public string?       Description       { get; private set; }
    public string?       Tags              { get; private set; }

    public IReadOnlyCollection<DocumentVersion> Versions
        => _versions.AsReadOnly();

    public DocumentVersion? CurrentVersion
        => _versions.FirstOrDefault(v => v.IsCurrentVersion);

    // ── Factory Method ─────────────────────────────────────────
    public static Document Create(
        Guid tenantId,
        Guid uploadedByUserId,
        DocumentTitle title,
        ContentType contentType,
        StoragePath storagePath,
        FileSize fileSize)
    {
        var document = new Document(
            Guid.NewGuid(),
            tenantId,
            uploadedByUserId,
            title,
            contentType,
            storagePath,
            fileSize);

        // Raise domain event — dispatched after DB commit
        var occurredOn = DateTime.UtcNow;
        document.RaiseDomainEvent(new DocumentCreatedEvent(
            Guid.NewGuid(),
            document.Id,
            tenantId,
            uploadedByUserId,
            title.Value,
            storagePath.Value,
            occurredOn,
            document.CreatedAt));

        return document;
    }

    // ── Status Transitions ─────────────────────────────────────
    // Only valid transitions are allowed
    // Trying to Approve an Uploading document throws
    public void MarkAsProcessing()
    {
        EnsureStatus(DocumentStatus.Uploading);
        ChangeStatus(DocumentStatus.Processing);
    }

    public void MarkAsActive()
    {
        EnsureStatus(DocumentStatus.Processing);
        ChangeStatus(DocumentStatus.Active);
    }

    public void SubmitForReview()
    {
        EnsureStatus(DocumentStatus.Active);
        ChangeStatus(DocumentStatus.UnderReview);
    }

    public void Approve()
    {
        EnsureStatus(DocumentStatus.UnderReview);
        ChangeStatus(DocumentStatus.Approved);
    }

    public void Reject()
    {
        EnsureStatus(DocumentStatus.UnderReview);
        ChangeStatus(DocumentStatus.Rejected);
    }

    public void Archive()
    {
        if (Status == DocumentStatus.Archived)
            throw new InvalidOperationException("Document is already archived");
        ChangeStatus(DocumentStatus.Archived);
    }

    // ── Version Management ─────────────────────────────────────
    public void AddVersion(
        StoragePath storagePath,
        FileSize fileSize,
        Guid uploadedByUserId)
    {
        // Only Active documents can have new versions uploaded
        if (Status != DocumentStatus.Active)
            throw new InvalidOperationException(
                $"Cannot add version to document with status {Status}");

        // Mark all existing versions as not current
        foreach (var v in _versions)
            v.IsCurrentVersion = false;

        var newVersionNumber = _versions.Count + 1;
        var newVersion = new DocumentVersion(
            Id,
            newVersionNumber,
            storagePath,
            fileSize,
            uploadedByUserId.ToString());

        newVersion.IsCurrentVersion = true;
        _versions.Add(newVersion);

        UpdatedAt = DateTime.UtcNow;

        var occurredOn = DateTime.UtcNow;
        RaiseDomainEvent(new DocumentVersionAddedEvent(
            Guid.NewGuid(),
            Id,
            TenantId,
            newVersionNumber,
            storagePath.Value,
            occurredOn,
            occurredOn));
    }

    // ── Metadata Updates ───────────────────────────────────────
    public void UpdateTitle(DocumentTitle newTitle)
    {
        Title     = newTitle;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt   = DateTime.UtcNow;
    }

    public void UpdateTags(string? tags)
    {
        Tags      = tags;
        UpdatedAt = DateTime.UtcNow;
    }

    // ── Private Helpers ────────────────────────────────────────
    private void ChangeStatus(DocumentStatus newStatus)
    {
        var oldStatus = Status;
        Status    = newStatus;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new DocumentStatusChangedEvent(
            Guid.NewGuid(),
            Id,
            TenantId,
            oldStatus,
            newStatus,
            UpdatedAt,
            UpdatedAt));
    }

    private void EnsureStatus(DocumentStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException(
                $"Document must be in {expected} status. Current: {Status}");
    }
}
