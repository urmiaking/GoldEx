using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesByVoucherIdSpecification : SpecificationBase<Invoice>
{
    public InvoicesByVoucherIdSpecification(PaymentVoucherId paymentVoucherId)
    {
        AddCriteria(x => x.InvoicePayments.Any(ip => ip.PaymentVoucherId == paymentVoucherId));
    }
}