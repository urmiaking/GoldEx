using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.LicensePaymentAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.LicensePayments;

public class LicensePaymentsDefaultSpecification : SpecificationBase<LicensePayment>
{
    public LicensePaymentsDefaultSpecification()
    {
        ApplyOrderBy(x => x.CreatedAt);
    }
}