using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;

public class FinancialAccountsByCustomerIdSpecification : SpecificationBase<FinancialAccount>
{
    public FinancialAccountsByCustomerIdSpecification(CustomerId customerId, PriceUnitId? priceUnitId = null)
    {
        AddCriteria(b => b.CustomerId == customerId);

        if (priceUnitId.HasValue)
            AddCriteria(b => b.PriceUnitId == priceUnitId);
    }
}