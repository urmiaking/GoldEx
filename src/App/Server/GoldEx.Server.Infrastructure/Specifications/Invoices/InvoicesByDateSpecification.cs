using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesByDateSpecification : SpecificationBase<Invoice>
{
    public InvoicesByDateSpecification(DateTime date)
    {
        var min = date.GetDayStart();
        var max = date.GetDayEnd();

        AddCriteria(x => x.CreatedAt >= min && x.CreatedAt <= max);
    }
}