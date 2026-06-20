using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.StoreAggregate;

public class StoreUser : EntityBase
{
    public StoreId StoreId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsDefault { get; private set; }

    public static StoreUser Create(StoreId storeId, Guid userId, bool isDefault = false)
    {
        return new StoreUser
        {
            StoreId = storeId,
            UserId = userId,
            IsDefault = isDefault
        };
    }

#pragma warning disable CS8618
    private StoreUser() { }
#pragma warning restore CS8618

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }
}
