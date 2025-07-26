using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;

public class PaymentVouchersByFinancialAccountIdSpecification : SpecificationBase<PaymentVoucher>
{
    public PaymentVouchersByFinancialAccountIdSpecification(FinancialAccountId financialAccountId)
    {
        AddCriteria(x => x.DestinationFinancialAccountId == financialAccountId);
    }
}