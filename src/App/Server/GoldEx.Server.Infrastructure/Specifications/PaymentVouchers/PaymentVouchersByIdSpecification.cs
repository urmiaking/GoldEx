using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PaymentVoucherAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;

public class PaymentVouchersByIdSpecification : SpecificationBase<PaymentVoucher>
{
    public PaymentVouchersByIdSpecification(PaymentVoucherId paymentVoucherId)
    {
        AddCriteria(x => x.Id == paymentVoucherId);

        // --- Includes ---
        AddInclude(x => x.SourceFinancialAccount!.PriceUnit!);
        AddInclude(x => x.DestinationFinancialAccount!.PriceUnit!);
        AddInclude(x => x.DestinationFinancialAccount!.Customer!);
        AddInclude(x => x.VoucherPriceUnit!);
    }
}