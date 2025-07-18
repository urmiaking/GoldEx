﻿using GoldEx.Sdk.Common.Domain;

namespace GoldEx.Sdk.Server.Domain.Entities;

public abstract class EntityBase : IHasDomainEvents
{
    public DateTime CreatedAt { get; protected set; } = DateTime.Now;

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