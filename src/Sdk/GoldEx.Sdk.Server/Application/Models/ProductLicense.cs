using GoldEx.Shared.Enums;

namespace GoldEx.Sdk.Server.Application.Models;

public class ProductLicense(LicensePlan plan, DateTime expireDate)
{
    public LicensePlan Plan { get; protected set; } = plan;
    public DateTime ExpireDate { get; protected set; } = expireDate;

    public void UpdateLicense(LicensePlan plan, DateTime expireDate)
    {
        Plan = plan;
        ExpireDate = expireDate;
    }
}