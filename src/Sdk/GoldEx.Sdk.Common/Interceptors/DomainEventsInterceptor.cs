using GoldEx.Sdk.Common.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GoldEx.Sdk.Common.Interceptors;

public class DomainEventsInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PublishPostPersistenceDomainEvents(eventData.Context).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await PublishPostPersistenceDomainEvents(eventData.Context);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        PublishPrePersistenceDomainEvents(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await PublishPrePersistenceDomainEvents(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task PublishPrePersistenceDomainEvents(DbContext? dbContext)
    {
        if (dbContext is null)
            return;

        var entities = dbContext.ChangeTracker.Entries<IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        foreach (var entity in entities)
        {
            var events = entity.DomainEvents.OfType<IPrePersistenceDomainEvent>().ToList();

            foreach (var e in events)
            {
                await publisher.Publish(e);
                entity.RemoveEvent(e);
            }
        }
    }

    private async Task PublishPostPersistenceDomainEvents(DbContext? dbContext)
    {
        if (dbContext is null)
            return;

        var entities = dbContext.ChangeTracker.Entries<IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        foreach (var entity in entities)
        {
            var events = entity.DomainEvents.OfType<IPostPersistenceDomainEvent>().ToList();
            
            foreach (var e in events)
            {
                await publisher.Publish(e);
                entity.RemoveEvent(e);
            }
        }
    }
}
