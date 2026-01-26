using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.InvoicePayments;

public class InvoicePaymentsByNumberSpecification : SpecificationBase<InvoicePayment>
{
    public InvoicePaymentsByNumberSpecification(long invoiceNumber, InvoiceType invoiceType)
    {
        AddInclude(x => x.Invoice!.Customer!);
        AddInclude(x => x.LedgerAccount!.Customer!);
        AddInclude(x => x.SourceFinancialAccount!);

        AddCriteria(x => x.Invoice!.InvoiceNumber == invoiceNumber);
        AddCriteria(x => x.Invoice!.InvoiceType == invoiceType);
    }
}