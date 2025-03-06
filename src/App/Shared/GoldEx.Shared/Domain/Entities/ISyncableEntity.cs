namespace GoldEx.Shared.Domain.Entities;

public interface ISyncableEntity
{
    DateTime LastModifiedDate { get; }
    void SetLastModifiedDate(DateTime date);
}