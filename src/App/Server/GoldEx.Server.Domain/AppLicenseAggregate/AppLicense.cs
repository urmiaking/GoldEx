using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.AppLicenseAggregate;

public class AppLicense : EntityBase
{
    private AppLicense() { }

    public static AppLicense Create(Guid licenseId, string? verificationKey = null)
    {
        return new AppLicense
        {
            LicenseId = licenseId,
            VerificationKey = verificationKey
        };
    }

    public Guid LicenseId { get; private set; }
    public string? VerificationKey { get; private set; }

    public void UpdateVerificationKey(string? verificationKey)
    {
        VerificationKey = verificationKey;
    }
}