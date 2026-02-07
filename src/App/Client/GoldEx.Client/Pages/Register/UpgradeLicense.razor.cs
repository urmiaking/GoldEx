using GoldEx.Client.Pages.Register.Components;
using GoldEx.Client.Pages.Register.ViewModels;
using GoldEx.Shared.DTOs.LicensePayments;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static GoldEx.Client.Pages.Register.Components.PaymentConfirmation;

namespace GoldEx.Client.Pages.Register;

public partial class UpgradeLicense
{
    [Parameter]
    public string? ModeParam { get; set; }

    private LicenseActionMode Mode { get; set; } = LicenseActionMode.Upgrade;

    private List<ProductUpgradeVm> SubscriptionPlans { get; set; } = [];
    private ProductUpgradeVm? SelectedPlan { get; set; }
    private bool SelectedPlanDisabled => SelectedPlan == null;

    private string PageTitleText =>
        Mode == LicenseActionMode.Upgrade
            ? "ارتقاء نسخه نرم افزار"
            : "تمدید اشتراک نرم افزار";

    private string HeaderText =>
        Mode == LicenseActionMode.Upgrade
            ? "ارتقاء نسخه"
            : "تمدید اشتراک";

    private string DescriptionText =>
        Mode == LicenseActionMode.Upgrade
            ? "نسخه خود را به نسخه کامل ارتقا دهید و از تمام امکانات نرم‌افزار بدون محدودیت استفاده کنید. با انتخاب اشتراک طولانی‌تر، تخفیف بیشتری دریافت کنید."
            : "اشتراک فعلی خود را تمدید کنید و بدون وقفه از تمام امکانات نرم‌افزار استفاده نمایید.";

    protected override void OnParametersSet()
    {
        var uri = Navigation.ToBaseRelativePath(Navigation.Uri).ToLowerInvariant();
        Mode = uri.Contains("extend-license")
            ? LicenseActionMode.Extend
            : LicenseActionMode.Upgrade;

        base.OnParametersSet();
    }

    protected override void OnInitialized()
    {
        LoadSubscriptionPlans();
        base.OnInitialized();
    }

    private void LoadSubscriptionPlans()
    {
        SubscriptionPlans =
        [
            new ProductUpgradeVm
            {
                Id = Guid.NewGuid(),
                Title = "اشتراک یک ماهه",
                DurationMonths = 1,
                Price = 2799000,
                OriginalPrice = 2799000,
                Benefits =
                [
                    "سرور قدرتمند و سریع",
                    "پشتیبانی فنی ۲۴ ساعته",
                    "به‌روزرسانی و پشتیبان گیری خودکار"
                ]
            },

            new ProductUpgradeVm
            {
                Id = Guid.NewGuid(),
                Title = "اشتراک سه ماهه",
                DurationMonths = 3,
                Price = 7559000,
                OriginalPrice = 8399000,
                Benefits =
                [
                    "سرور قدرتمند و سریع",
                    "پشتیبانی فنی ۲۴ ساعته",
                    "به‌روزرسانی و پشتیبان گیری خودکار"
                ]
            },

            new ProductUpgradeVm
            {
                Id = Guid.NewGuid(),
                Title = "اشتراک شش ماهه",
                DurationMonths = 6,
                Price = 15999000,
                OriginalPrice = 19999000,
                Benefits =
                [
                    "سرور قدرتمند و سریع",
                    "پشتیبانی فنی ۲۴ ساعته",
                    "به‌روزرسانی و پشتیبان گیری خودکار"
                ]
            },

            new ProductUpgradeVm
            {
                Id = Guid.NewGuid(),
                Title = "اشتراک یک ساله",
                DurationMonths = 12,
                Price = 23499000,
                OriginalPrice = 32549000,
                Benefits =
                [
                    "سرور قدرتمند و سریع",
                    "پشتیبانی فنی ۲۴ ساعته",
                    "به‌روزرسانی و پشتیبان گیری خودکار"
                ]
            }
        ];
    }

    private void SelectPlan(Guid planId)
    {
        foreach (var plan in SubscriptionPlans)
        {
            plan.IsSelected = plan.Id == planId;
        }
        SelectedPlan = SubscriptionPlans.FirstOrDefault(p => p.Id == planId);
        StateHasChanged();
    }

    private async Task SubmitUpgradeRequest()
    {
        if (SelectedPlan == null) return;

        var parameters = new DialogParameters<PaymentConfirmation>
        {
            { x => x.Plan, SelectedPlan }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        var dialog = await DialogService.ShowAsync<PaymentConfirmation>(
            "تأیید پرداخت", parameters, options);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: PaymentInfoVm paymentInfoVm })
        {
            var request = new LicensePaymentRequest(SelectedPlan.DurationMonths, paymentInfoVm.ReferenceNumber, paymentInfoVm.Description);

            await SendRequestAsync<ILicensePaymentService>(
                action: (s, ct) => s.CreateAsync(request, ct),
                afterSend: () =>
                {
                    AddSuccessToast("درخواست ارتقاء شما با موفقیت ثبت شد. پس از بررسی پرداخت، اشتراک شما فعال خواهد شد.");
                    Navigation.NavigateTo(ClientRoutes.Home.Index);
                    return Task.CompletedTask;
                });
        }
    }

    private string GetPlanIcon(int months)
    {
        return months switch
        {
            1 => Icons.Material.Filled.Schedule,
            3 => Icons.Material.Filled.DateRange,
            6 => Icons.Material.Filled.CalendarMonth,
            12 => Icons.Material.Filled.Star,
            _ => Icons.Material.Filled.QuestionMark    // fallback
        };
    }
}