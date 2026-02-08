using GoldEx.Client.Components.Services;
using GoldEx.Shared.DTOs.LicensePayments;
using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Settings;

public partial class LicenseRequests
{
    [Inject] public LicenseState LicenseState { get; set; } = default!;

    private List<LicensePaymentResponse> _requests = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadReportsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadReportsAsync()
    {
        await SendRequestAsync<ILicensePaymentService, List<LicensePaymentResponse>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response => _requests = response);
    }

    private async Task OnSetStatus(LicensePaymentResponse context, LicensePaymentStatus status)
    {
        var result = await DialogService.ShowMessageBox(
            "هشدار",
            "آیا مطمئن هستید؟",
            yesText: "بله", cancelText: "لغو");

        if (result is true)
        {
            await SendRequestAsync<ILicensePaymentService>(
                action: (s, ct) => s.SetStatusAsync(context.Id, status, ct),
                afterSend: async () =>
                {
                    AddSuccessToast("عملیات با موفقیت انجام شد");
                    await LoadReportsAsync();
                    await RefreshLicenseAsync();
                });
        }
    }

    private async Task RefreshLicenseAsync()
    {
        var license = await SendRequestAsync<ILicenseService, GetLicenseResponse>(
            action: (s, ct) => s.GetLicenseAsync(ct),
            createScope: true);

        LicenseState.Set(license);
    }
}