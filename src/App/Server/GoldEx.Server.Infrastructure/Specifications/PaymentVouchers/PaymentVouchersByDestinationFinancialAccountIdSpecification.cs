using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;

public class PaymentVouchersByDestinationFinancialAccountIdSpecification : SpecificationBase<PaymentVoucher>
{
    public PaymentVouchersByDestinationFinancialAccountIdSpecification(FinancialAccountId financialAccountId)
    {
        AddCriteria(x => x.DestinationFinancialAccountId == financialAccountId);
    }
}