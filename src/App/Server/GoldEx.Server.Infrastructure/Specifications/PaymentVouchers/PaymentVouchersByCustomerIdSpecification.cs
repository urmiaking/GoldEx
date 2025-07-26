using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;

public class PaymentVouchersByCustomerIdSpecification : SpecificationBase<PaymentVoucher>
{
    public PaymentVouchersByCustomerIdSpecification(CustomerId customerId)
    {
        AddInclude(x => x.DestinationFinancialAccount!);
        AddCriteria(x => x.DestinationFinancialAccount!.CustomerId == customerId);
        ApplyOrderByDescending(x => x.CreatedAt);
    }
}