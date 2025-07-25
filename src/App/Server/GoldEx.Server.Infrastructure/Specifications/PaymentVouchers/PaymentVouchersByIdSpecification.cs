using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PaymentVoucherAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;

public class PaymentVouchersByIdSpecification : SpecificationBase<PaymentVoucher>
{
    public PaymentVouchersByIdSpecification(PaymentVoucherId paymentVoucherId)
    {
        AddCriteria(x => x.Id == paymentVoucherId);
    }
}