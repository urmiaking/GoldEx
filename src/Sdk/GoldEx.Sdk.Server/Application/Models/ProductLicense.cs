using GoldEx.Shared.Enums;

namespace GoldEx.Sdk.Server.Application.Models;

public class ProductLicense
{
    public Guid StoreId { get; private set; }
    public LicensePlan Plan { get; private set; }
    public DateTime ExpireDate { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    public void UpdateLicense(Guid storeId, LicensePlan plan, DateTime registeredAt, DateTime expireDate)
    {
        StoreId = storeId;
        Plan = plan;
        ExpireDate = expireDate;
        RegisteredAt = registeredAt;
    }
}