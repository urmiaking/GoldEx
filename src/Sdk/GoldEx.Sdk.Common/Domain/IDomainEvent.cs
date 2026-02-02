namespace GoldEx.Sdk.Common.Domain;

public interface IDomainEvent;

public interface IPrePersistenceDomainEvent : IDomainEvent;
public interface IPostPersistenceDomainEvent : IDomainEvent;