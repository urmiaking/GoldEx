using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MudBlazor;

namespace GoldEx.Client.Components.Layouts.Components;

public partial class LicenseStatus
{
    private GetLicenseResponse? _licenseInfo;

    protected int RemainingDays =>
        _licenseInfo is null
            ? 0
            : (int)Math.Ceiling(
                (_licenseInfo.ExpireDate.Date - DateTime.Now.Date).TotalDays);

    protected override async Task OnInitializedAsync()
    {
        await LoadLicenseAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadLicenseAsync()
    {
        await SendRequestAsync<ILicenseService, GetLicenseResponse>(
            action: (s, ct) => s.GetLicenseAsync(ct),
            afterSend: response => _licenseInfo = response,
            createScope: true);
    }

    private Color GetLicenseColor(GetLicenseResponse licenseInfo)
    {
        return licenseInfo.Plan switch
        {
            LicensePlan.Unregistered => Color.Error,
            LicensePlan.Trial => Color.Info,
            LicensePlan.Regular => Color.Success,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private double GetLicenseProgress(GetLicenseResponse license)
    {
        if (license.Plan == LicensePlan.Unregistered)
            return 0;

        if (license.ExpireDate == DateTime.MinValue)
            return 0;

        var now = DateTime.Now.Date;
        var start = license.RegisteredAt.Date;
        var end = license.ExpireDate.Date;

        if (now >= end || start >= end)
            return 0;

        var totalDays = (end - start).TotalDays;
        var remainingDays = (end - now).TotalDays;

        return Math.Clamp((remainingDays / totalDays) * 100, 0, 100);
    }

}