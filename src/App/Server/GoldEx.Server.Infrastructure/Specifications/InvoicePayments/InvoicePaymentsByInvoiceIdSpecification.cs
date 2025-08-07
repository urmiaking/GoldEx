using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InvoicePayments;

public class InvoicePaymentsByInvoiceIdSpecification : SpecificationBase<InvoicePayment>
{
    public InvoicePaymentsByInvoiceIdSpecification(InvoiceId invoiceId)
    {
        AddCriteria(x => x.InvoiceId == invoiceId);
    }   
}