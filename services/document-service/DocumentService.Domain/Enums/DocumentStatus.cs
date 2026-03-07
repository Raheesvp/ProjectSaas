namespace DocumentService.Domain.Enums;

public enum DocumentStatus
{
    Uploading   = 1,  // Chunks being received — not yet complete
    Processing  = 2,  // OCR + metadata extraction in progress
    Active      = 3,  // Ready for workflow / viewing
    UnderReview = 4,  // Workflow approval in progress
    Approved    = 5,  // Final approved state
    Rejected    = 6,  // Rejected by approver
    Archived    = 7   // Soft-deleted — hidden but not removed
}