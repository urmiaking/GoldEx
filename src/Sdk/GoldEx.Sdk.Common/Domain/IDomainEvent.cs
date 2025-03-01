using MediatR;

namespace GoldEx.Sdk.Common.Domain;

public interface IDomainEvent : INotification { }
public interface IPrePersistenceDomainEvent : IDomainEvent, INotification { }
public interface IPostPersistenceDomainEvent : IDomainEvent, INotification { }
