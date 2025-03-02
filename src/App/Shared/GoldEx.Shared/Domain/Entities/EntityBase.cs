using GoldEx.Sdk.Common.Domain;

namespace GoldEx.Shared.Domain.Entities;

public abstract class EntityBase : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    public void RemoveEvent(IDomainEvent e)
    {
        _domainEvents.Remove(e);
    }
}

public abstract class EntityBase<TId> : EntityBase
    where TId : notnull
{
    public TId Id { get; protected set; }

    protected EntityBase(TId id)
    {
        Id = id;
    }

#pragma warning disable CS8618 
    protected EntityBase() { }
#pragma warning restore CS8618 
}

public abstract class SyncableEntityBase<TId> : EntityBase<TId>
    where TId : notnull
{
    public bool IsModified { get; protected set; }
    public DateTime? LastModifiedDate { get; protected set; }
    public bool IsSynced { get; protected set; }

    protected SyncableEntityBase(TId id) : base(id) { }

#pragma warning disable CS8618
    protected SyncableEntityBase() { }
#pragma warning restore CS8618

    public void SetModified(bool isModified) => IsModified = isModified;
    public void SetLastModifiedDate(DateTime date) => LastModifiedDate = date;
    public void SetSynced(bool isSynced) => IsSynced = isSynced;
}