using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesByProductIdSpecification : SpecificationBase<Invoice>
{
    public InvoicesByProductIdSpecification(ProductId productId, InvoiceType? invoiceType = null)
    {
        AddCriteria(x => x.ProductItems.Any(i => i.ProductId == productId) || x.UsedProducts.Any(y => y.ProductId == productId));

        if (invoiceType.HasValue)
        {
            AddCriteria(x => x.InvoiceType == invoiceType.Value);
        }

        ApplyOrderByDescending(x => x.InvoiceDate);
    }
}