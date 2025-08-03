using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;

public class PaymentVouchersBySourceFinancialAccountIdSpecification : SpecificationBase<PaymentVoucher>
{
    public PaymentVouchersBySourceFinancialAccountIdSpecification(FinancialAccountId financialAccountId)
    {
        AddCriteria(x => x.SourceFinancialAccountId == financialAccountId);
    }
}