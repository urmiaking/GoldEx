using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.LicensePaymentAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.LicensePayments;

public class LicensePaymentsByStatusSpecification : SpecificationBase<LicensePayment>
{
    public LicensePaymentsByStatusSpecification(LicensePaymentStatus status)
    {
        AddCriteria(x => x.Status == status);
    }   
}