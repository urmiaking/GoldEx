using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceItemAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InvoiceItems;

public class InvoiceItemsByIdSpecification : SpecificationBase<InvoiceItem>
{
    public InvoiceItemsByIdSpecification(InvoiceItemId id)
    {
        AddCriteria(x => x.Id == id);
    }
}