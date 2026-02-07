using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Licenses;

public record GetLicenseResponse(LicensePlan Plan, DateTime RegisteredAt, DateTime ExpireDate)
{
    public bool IsExpired => ExpireDate < DateTime.Now && Plan == LicensePlan.Trial;
}