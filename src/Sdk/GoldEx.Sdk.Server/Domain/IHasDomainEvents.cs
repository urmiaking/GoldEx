namespace GoldEx.Sdk.Server.Domain;

public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    void RemoveEvent(IDomainEvent e);
}
