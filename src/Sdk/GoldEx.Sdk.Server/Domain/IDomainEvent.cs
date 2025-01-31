using MediatR;

namespace GoldEx.Sdk.Server.Domain;

public interface IDomainEvent : INotification { }
public interface IPrePersistenceDomainEvent : IDomainEvent, INotification { }
public interface IPostPersistenceDomainEvent : IDomainEvent, INotification { }
