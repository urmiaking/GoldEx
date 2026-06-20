using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Domain.Common;

public interface IStoreFiltered
{
    StoreId StoreId { get; }
}
