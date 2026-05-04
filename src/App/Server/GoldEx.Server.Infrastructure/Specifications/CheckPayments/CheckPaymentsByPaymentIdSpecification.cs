using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CheckPaymentAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.CheckPayments;

public class CheckPaymentsByPaymentIdSpecification : SpecificationBase<CheckPayment>
{
    public CheckPaymentsByPaymentIdSpecification(InvoicePaymentId paymentId)
    {
        AddCriteria(x => x.InvoicePaymentId == paymentId);
    }
}