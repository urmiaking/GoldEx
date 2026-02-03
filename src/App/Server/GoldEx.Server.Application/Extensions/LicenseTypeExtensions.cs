using GoldEx.Shared.Enums;
using VHDLicenseManager.Responses;

namespace GoldEx.Server.Application.Extensions;

public static class LicenseTypeExtensions
{
    public static LicensePlan GetLicensePlan(this LicenseType licenseType)
    {
        return licenseType switch {
            LicenseType.Trial => LicensePlan.Trial,
            LicenseType.Regular => LicensePlan.Regular,
            _ => throw new ArgumentOutOfRangeException(nameof(licenseType), licenseType, null)
        };
    }
}