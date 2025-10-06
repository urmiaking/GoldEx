using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CoinAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Coins;

public class CoinsByIdSpecification : SpecificationBase<Coin>
{
    public CoinsByIdSpecification(CoinId id)
    {
        AddCriteria(x => x.Id == id);
    }    
}