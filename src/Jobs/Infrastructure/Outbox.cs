using System.Text.Json;
using JobTrackerService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace JobTrackerService.Jobs.Infrastructure;

public sealed class InsertOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is JobsDbContext db)
        {
            var aggregates = db.ChangeTracker.Entries<AggregateRoot<Guid>>().Select(entry => entry.Entity).ToList();
            foreach (var aggregate in aggregates)
                foreach (var domainEvent in aggregate.DomainEvents)
                    db.OutboxMessages.Add(new OutboxMessage { Id = Guid.NewGuid(), Type = domainEvent.GetType().AssemblyQualifiedName!,
                        Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()), OccurredOn = domainEvent.OccurredOn });
            foreach (var aggregate in aggregates) aggregate.ClearDomainEvents();
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

public sealed record JobCompletedIntegrationEvent(Guid JobId, Guid CustomerId, DateTime CompletedAt, string IdempotencyKey);

public sealed class OutboxDispatcher(JobsDbContext db)
{
    public async Task DispatchAsync(CancellationToken cancellationToken = default)
    {
        var pending = await db.OutboxMessages.Where(x => x.ProcessedOn == null).OrderBy(x => x.OccurredOn).Take(100).ToListAsync(cancellationToken);
        foreach (var message in pending) { message.ProcessedOn = DateTime.UtcNow; }
        await db.SaveChangesAsync(cancellationToken);
    }
}