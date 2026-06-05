using System.Linq;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CheckPaymentAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.CheckPayments;

public class CheckPaymentsByIdSpecification : SpecificationBase<CheckPayment>
{
    public CheckPaymentsByIdSpecification(CheckPaymentId id)
    {
        AddCriteria(x => x.Id == id);

        AddInclude(x => x.Issuer!);
        AddInclude(x => x.IssuerFinancialAccount!);
        AddInclude(x => x.InvoicePayment!);
        AddInclude(x => x.InvoicePayment!.PriceUnit!);
        AddInclude(x => x.InvoicePayment!.Invoice!);
        AddInclude(x => x.ChangeLogs);
    }
}
