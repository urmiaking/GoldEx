using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.AppLicenseAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.AppLicenses;

public class AppLicensesDefaultSpecification : SpecificationBase<AppLicense>
{
    public AppLicensesDefaultSpecification()
    {
        ApplyOrderBy(x => x.LicenseId);
    }
}