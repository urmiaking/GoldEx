namespace GoldEx.Sdk.Common.Domain;

public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    void RemoveEvent(IDomainEvent e);
}
