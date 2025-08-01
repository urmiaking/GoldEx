using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;

public class PaymentVouchersByCustomerIdSpecification : SpecificationBase<PaymentVoucher>
{
    public PaymentVouchersByCustomerIdSpecification(CustomerId customerId, VoucherStatus? status = VoucherStatus.Pending)
    {
        AddInclude(x => x.SourceFinancialAccount!.PriceUnit!);
        AddInclude(x => x.DestinationFinancialAccount!.Customer!);
        AddInclude(x => x.DestinationFinancialAccount!.PriceUnit!);
        AddInclude(x => x.VoucherPriceUnit!);

        AddCriteria(x => x.DestinationFinancialAccount!.CustomerId == customerId);
        
        //TODO: add status filtering

        ApplyOrderByDescending(x => x.CreatedAt);
    }
}