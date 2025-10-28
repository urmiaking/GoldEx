using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Transactions;

public class TransactionsByInvoiceIdSpecification : SpecificationBase<Transaction>
{
    // includePayments: اگر false باشد، پرداخت‌ها (دارای InvoicePaymentId) برگردانده نمی‌شوند
    public TransactionsByInvoiceIdSpecification(InvoiceId invoiceId, bool includePayments = true)
    {
        AddCriteria(x => x.InvoiceId == invoiceId);

        if (!includePayments)
        {
            AddCriteria(x => x.InvoicePaymentId == null);
        }

        ApplyOrderBy(x => x.PostingDate);
    }
}