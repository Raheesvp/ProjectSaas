using DocumentService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Domain.Primitives;

namespace DocumentService.Infrastructure.Persistence.Interceptors;

// DomainEventInterceptor — automatically dispatches domain events
// after every SaveChangesAsync call
//
// How it works:
// 1. EF Core calls SaveChangesAsync
// 2. Before committing — this interceptor fires
// 3. It finds all AggregateRoot entities in the change tracker
// 4. Collects all pending domain events from them
// 5. Clears the events from the aggregates
// 6. Lets SaveChangesAsync commit to DB
// 7. Then publishes all events via MediatR
//
// Real world: This pattern ensures events are only published
// AFTER the database transaction succeeds
// If DB fails — no events are published — system stays consistent
public sealed class DomainEventInterceptor
    : SaveChangesInterceptor
{
    private readonly IMediator _mediator;

    public DomainEventInterceptor(IMediator mediator)
        => _mediator = mediator;

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(eventData.Context);
        return await base.SavedChangesAsync(
            eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(DbContext? context)
    {
        if (context is null) return;

        // Find all aggregate roots tracked by EF Core
        // that have pending domain events
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // Collect all events before clearing
        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // Clear events from all aggregates
        // Prevents double-publishing if SaveChanges called twice
        aggregates.ForEach(a => a.ClearDomainEvents());

        // Publish each event via MediatR
        // In-process handlers pick these up immediately
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent);
        }
    }
}