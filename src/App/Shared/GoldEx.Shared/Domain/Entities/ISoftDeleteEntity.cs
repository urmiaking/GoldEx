namespace GoldEx.Shared.Domain.Entities;

public interface ISoftDeleteEntity
{
    bool IsDeleted { get; }

    void SetDeleted();
}