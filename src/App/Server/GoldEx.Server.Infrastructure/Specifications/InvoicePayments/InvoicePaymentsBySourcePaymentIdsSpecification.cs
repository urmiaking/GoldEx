using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoicePaymentAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InvoicePayments;

public class InvoicePaymentsBySourcePaymentIdsSpecification : SpecificationBase<InvoicePayment>
{
    public InvoicePaymentsBySourcePaymentIdsSpecification(IEnumerable<InvoicePaymentId> paymentIds)
    {
        AddCriteria(x =>
            x.SourcePaymentId != null &&
            paymentIds.Contains(x.SourcePaymentId.Value));
    }
}