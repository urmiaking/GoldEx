using GoldEx.Shared.Enums;

namespace GoldEx.Sdk.Server.Application.Models;

public class ProductLicense
{
    public LicensePlan Plan { get; private set; }
    public DateTime ExpireDate { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    public void UpdateLicense(LicensePlan plan, DateTime registeredAt, DateTime expireDate)
    {
        Plan = plan;
        ExpireDate = expireDate;
        RegisteredAt = registeredAt;
    }
}