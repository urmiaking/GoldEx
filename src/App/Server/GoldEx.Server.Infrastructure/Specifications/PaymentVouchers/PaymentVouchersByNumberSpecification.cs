using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PaymentVoucherAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;

public class PaymentVouchersByNumberSpecification : SpecificationBase<PaymentVoucher>
{
    public PaymentVouchersByNumberSpecification(long number)
    {
        AddCriteria(x => x.VoucherNumber == number);
    }
}