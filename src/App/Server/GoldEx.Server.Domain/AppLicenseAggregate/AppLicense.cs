using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.AppLicenseAggregate;

public class AppLicense : EntityBase
{
    public static AppLicense Create(Guid licenseId)
    {
        return new AppLicense
        {
            LicenseId = licenseId
        };
    }

    public Guid LicenseId { get; private set; }
    public string? VerificationKey { get; private set; }

    public void Update(Guid licenseId, string? verificationKey)
    {
        LicenseId = licenseId;
        VerificationKey = verificationKey;
    }
}