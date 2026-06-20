using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.AppLicenseAggregate;

public class AppLicense : EntityBase, IStoreFiltered
{
    private AppLicense() { }

    public static AppLicense Create(
        Guid licenseId, 
        string? verificationKey = null, 
        StoreId storeId = default, 
        LicensePlan plan = LicensePlan.Unregistered, 
        DateTime? registeredAt = null, 
        DateTime? expireDate = null)
    {
        return new AppLicense
        {
            LicenseId = licenseId,
            VerificationKey = verificationKey,
            StoreId = storeId,
            Plan = plan,
            RegisteredAt = registeredAt ?? DateTime.MinValue,
            ExpireDate = expireDate ?? DateTime.MinValue
        };
    }

    public StoreId StoreId { get; private set; }
    public Guid LicenseId { get; private set; }
    public string? VerificationKey { get; private set; }
    public LicensePlan Plan { get; private set; }
    public DateTime ExpireDate { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    public void UpdateVerificationKey(string? verificationKey)
    {
        VerificationKey = verificationKey;
    }

    public void UpdateSubscription(LicensePlan plan, DateTime registeredAt, DateTime expireDate)
    {
        Plan = plan;
        RegisteredAt = registeredAt;
        ExpireDate = expireDate;
    }
}