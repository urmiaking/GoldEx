using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Transactions;

public class TransactionsByPaymentVoucherIdSpecification : SpecificationBase<Transaction>
{
    public TransactionsByPaymentVoucherIdSpecification(PaymentVoucherId voucherId)
    {
        AddCriteria(x => x.PaymentVoucherId == voucherId);
        ApplyOrderBy(x => x.PostingDate);
    }
}