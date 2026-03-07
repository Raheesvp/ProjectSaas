using Shared.Domain.Events;
using DocumentService.Domain.Enums;

namespace DocumentService.Domain.Events;

// Raised when document moves between statuses
// e.g. Processing → Active, Active → UnderReview, UnderReview → Approved
// Consumed by: Notification Service, Audit Service, SignalR Hub
public sealed record DocumentStatusChangedEvent(
    Guid EventId,
    Guid DocumentId,
    Guid TenantId,
    DocumentStatus OldStatus,
    DocumentStatus NewStatus,
    DateTime OccuredOn,
    DateTime ChangedAt) : IDomainEvent;