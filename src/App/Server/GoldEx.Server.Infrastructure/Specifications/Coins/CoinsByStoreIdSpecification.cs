using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Coins;

public class CoinsByStoreIdSpecification : SpecificationBase<Coin>
{
    public CoinsByStoreIdSpecification(StoreId storeId)
    {
        AddCriteria(x => x.StoreId == storeId);
    }
}
