using GoldEx.Client.Components.Services;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Components.Layouts.Components;

public partial class LicenseStatusBanner
{
    [Inject] public LicenseState LicenseState { get; set; } = default!;

    protected const int WarningDays = 7;

    protected int RemainingDays =>
        LicenseState.Current is null
            ? 0
            : Math.Max(0, (int)Math.Ceiling(
                (LicenseState.Current.ExpireDate.Date - DateTime.Now.Date).TotalDays));

    protected double ProgressValue =>
        Math.Clamp((RemainingDays / (double)WarningDays) * 100, 0, 100);

    protected override void OnInitialized()
    {
        LicenseState.OnChange += StateHasChanged;
    }

    public override ValueTask DisposeAsync()
    {
        LicenseState.OnChange -= StateHasChanged;
        return base.DisposeAsync();
    }
}