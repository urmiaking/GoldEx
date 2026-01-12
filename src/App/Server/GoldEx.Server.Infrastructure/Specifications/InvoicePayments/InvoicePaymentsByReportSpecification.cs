using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.DTOs.Reporting;

namespace GoldEx.Server.Infrastructure.Specifications.InvoicePayments;

public class InvoicePaymentsByReportSpecification : SpecificationBase<InvoicePayment>
{
    public InvoicePaymentsByReportSpecification(PaymentRpRequest request)
    {
        AddInclude(x => x.Invoice!.Customer!);
        AddInclude(x => x.LedgerAccount!.Customer!);
        AddInclude(x => x.SourceFinancialAccount!);

        if (request.FromDate.HasValue)
        {
            AddCriteria(x => x.PaymentDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            AddCriteria(x => x.PaymentDate <= request.ToDate.Value);
        }

        if (request.CustomerId.HasValue)
        {
            AddCriteria(x => x.Invoice!.CustomerId == new CustomerId(request.CustomerId.Value));
        }

        if (request.PriceUnitId.HasValue)
        {
            AddCriteria(x => x.PriceUnitId == new PriceUnitId(request.PriceUnitId.Value));
        }

        if (request.PaymentType.HasValue)
        {
            AddCriteria(x => x.PaymentType == request.PaymentType.Value);
        }

        if (request.PaymentSide.HasValue)
        {
            AddCriteria(x => x.PaymentSide == request.PaymentSide.Value);
        }

        ApplyOrderBy(x => x.PaymentDate);
    }
}