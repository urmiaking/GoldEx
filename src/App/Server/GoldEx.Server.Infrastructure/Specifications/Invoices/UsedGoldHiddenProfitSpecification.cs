using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using System.Linq;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class UsedGoldHiddenProfitSpecification : SpecificationBase<Invoice>
{
    public UsedGoldHiddenProfitSpecification(
        Guid? customerId = null,
        Guid? priceUnitId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        // Filter invoices that have at least one used product
        AddCriteria(x => x.UsedProducts.Any());

        if (customerId.HasValue)
        {
            AddCriteria(x => x.CustomerId == new CustomerId(customerId.Value));
        }

        if (priceUnitId.HasValue)
        {
            AddCriteria(x => x.PriceUnitId == new PriceUnitId(priceUnitId.Value));
        }

        if (fromDate.HasValue)
        {
            var from = DateOnly.FromDateTime(fromDate.Value);
            AddCriteria(x => x.InvoiceDate >= from);
        }

        if (toDate.HasValue)
        {
            var to = DateOnly.FromDateTime(toDate.Value);
            AddCriteria(x => x.InvoiceDate <= to);
        }

        ApplyOrderBy(x => x.InvoiceDate);
    }
}
