using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Transactions;

public class TransactionsByInvoicePaymentIdSpecification : SpecificationBase<Transaction>
{
    public TransactionsByInvoicePaymentIdSpecification(InvoicePaymentId paymentId)
    {
        AddCriteria(x => x.InvoicePaymentId == paymentId);
    }
}