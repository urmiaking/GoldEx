using GoldEx.Shared.Enums;

namespace GoldEx.Sdk.Server.Application.Models;

public class ProductLicense
{
    public LicensePlan Plan { get; private set; }
    public DateTime ExpireDate { get; private set; }

    public void UpdateLicense(LicensePlan plan, DateTime expireDate)
    {
        Plan = plan;
        ExpireDate = expireDate;
    }
}