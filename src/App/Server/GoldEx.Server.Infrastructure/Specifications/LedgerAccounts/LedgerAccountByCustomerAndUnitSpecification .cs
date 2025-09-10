using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;

public class LedgerAccountByCustomerAndUnitSpecification : SpecificationBase<LedgerAccount>
{
    public LedgerAccountByCustomerAndUnitSpecification(
        CustomerId customerId,
        LedgerAccountId parentId,
        PriceUnitId priceUnitId)
    {
        AddCriteria(la => la.CustomerId == customerId &&
                          la.ParentAccountId == parentId &&
                          la.PriceUnitId == priceUnitId);
    }
}