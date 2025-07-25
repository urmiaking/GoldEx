using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;

public class PaymentVouchersByCustomerIdSpecification : SpecificationBase<PaymentVoucher>
{
    public PaymentVouchersByCustomerIdSpecification(CustomerId customerId)
    {
        AddCriteria(x => x.CustomerId == customerId);
        ApplyOrderByDescending(x => x.CreatedAt);
    }
}