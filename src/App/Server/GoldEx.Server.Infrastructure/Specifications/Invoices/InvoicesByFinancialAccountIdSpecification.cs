using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesByFinancialAccountIdSpecification : SpecificationBase<Invoice>
{
    public InvoicesByFinancialAccountIdSpecification(FinancialAccountId financialAccountId)
    {
        AddCriteria(x => x.InvoicePayments.Any(ip => ip.SourceFinancialAccountId == financialAccountId));
    }
}