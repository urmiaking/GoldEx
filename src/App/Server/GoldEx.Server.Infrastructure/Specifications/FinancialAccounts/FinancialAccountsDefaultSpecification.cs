using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;

public class FinancialAccountsDefaultSpecification : SpecificationBase<FinancialAccount>
{
    public FinancialAccountsDefaultSpecification(bool? isSystemAccount = false, PriceUnitId? priceUnitId = null)
    {
        AddCriteria(x => isSystemAccount == null || x.IsSystemAccount == isSystemAccount);
        if (priceUnitId.HasValue)
        {
            AddCriteria(x => x.PriceUnitId == priceUnitId);
        }

        ApplyOrderByDescending(x => x.CreatedAt);
    }
}