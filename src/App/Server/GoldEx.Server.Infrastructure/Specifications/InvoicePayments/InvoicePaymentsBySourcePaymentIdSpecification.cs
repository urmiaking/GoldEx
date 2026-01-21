using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoicePaymentAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InvoicePayments;

public class InvoicePaymentsBySourcePaymentIdSpecification : SpecificationBase<InvoicePayment>
{
    public InvoicePaymentsBySourcePaymentIdSpecification(InvoicePaymentId sourcePaymentId)
    {
        AddCriteria(x => x.SourcePaymentId == sourcePaymentId);
    }
}