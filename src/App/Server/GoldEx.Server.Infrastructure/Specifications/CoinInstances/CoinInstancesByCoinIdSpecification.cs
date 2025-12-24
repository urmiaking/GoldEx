using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.CoinInstanceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.CoinInstances;

public class CoinInstancesByCoinIdSpecification : SpecificationBase<CoinInstance>
{
    public CoinInstancesByCoinIdSpecification(CoinId coinId)
    {
        AddCriteria(x => x.CoinId == coinId);
    }
}