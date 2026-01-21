using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InvoicePayments;

public class InvoicePaymentsByTargetInvoiceIdSpecification : SpecificationBase<InvoicePayment>
{
    public InvoicePaymentsByTargetInvoiceIdSpecification(InvoiceId invoiceId)
    {
        AddCriteria(x => x.TargetInvoiceId == invoiceId);
    }
}