using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CoinAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Coins;

public class CoinsByStatusSpecification : SpecificationBase<Coin>
{
    public CoinsByStatusSpecification(bool? isActive = null)
    {
        if (isActive.HasValue)
        {
            AddCriteria(x => x.IsActive == isActive.Value);
        }
    }
}