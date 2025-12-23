using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class UnpaidPriceSelector
{
    [Parameter, EditorRequired] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];
    [Parameter] public GetPriceUnitTitleResponse? MainPriceUnit { get; set; }

    [Parameter] public decimal TotalUnpaidAmount { get; set; }
    [Parameter] public decimal? TotalUnpaidSecondaryAmount { get; set; }

    [Parameter] public GetPriceUnitTitleResponse? UnpaidPriceUnit { get; set; }
    [Parameter] public EventCallback<GetPriceUnitTitleResponse?> UnpaidPriceUnitChanged { get; set; }

    // ExchangeRate همیشه در جهت Main -> Secondary ذخیره می‌شود.
    [Parameter] public decimal? ExchangeRate { get; set; }
    [Parameter] public EventCallback<decimal?> ExchangeRateChanged { get; set; }

    private string SelectedMode
    {
        get => UnpaidPriceUnit is null ? "main" : "custom";
        set
        {
            if (value == "main")
            {
                _ = SetUnpaidPriceUnitAsync(null);
                _ = SetExchangeRateAsync(null);
            }
            else
            {
                if (UnpaidPriceUnit is null && MainPriceUnit is not null)
                {
                    var defaultUnit = MainPriceUnit.IsDefault
                        ? PriceUnits.FirstOrDefault(pu => pu.IsGoldBased)
                        : PriceUnits.FirstOrDefault(pu => pu.IsDefault);
                    _ = SetUnpaidPriceUnitAsync(defaultUnit);
                }
            }
        }
    }

    private GetPriceUnitTitleResponse? BoundUnpaidPriceUnit
    {
        get => UnpaidPriceUnit;
        set => _ = SetUnpaidPriceUnitAsync(value);
    }

    private static bool IsMoney(GetPriceUnitTitleResponse? unit) =>
        unit is not null && unit.IsGoldBased == false;

    // معکوس نمایش: تومان→گرم یا پول→پول با نرخ کوچک
    private bool ShouldReverseDisplay =>
        (MainPriceUnit?.IsDefault == true && UnpaidPriceUnit?.IsGoldBased == true)
        || (IsMoney(MainPriceUnit) && IsMoney(UnpaidPriceUnit) && ExchangeRate.HasValue && ExchangeRate.Value < 1m);

    // نرخ نمایشی خام (با لحاظ معکوس‌سازی)
    private decimal? RawDisplayExchangeRate =>
        ExchangeRate.HasValue
            ? (ShouldReverseDisplay
                ? (ExchangeRate == 0 ? null : 1 / ExchangeRate.Value)
                : ExchangeRate)
            : null;

    private int RateDisplayDecimals => 0;

    private decimal? RoundedDisplayExchangeRate =>
        RawDisplayExchangeRate.HasValue
            ? decimal.Round(RawDisplayExchangeRate.Value, RateDisplayDecimals, MidpointRounding.AwayFromZero)
            : null;

    // پراکسی برای @bind-Value فیلد عددی
    private decimal? BoundDisplayExchangeRate
    {
        get => RoundedDisplayExchangeRate;
        set
        {
            if (value is null)
            {
                _ = SetExchangeRateAsync(null);
                return;
            }

            if (value <= 0)
            {
                // نرخ نامعتبر – می‌توان پیام اعتبارسنجی اضافه کرد
                return;
            }

            // تبدیل ورودی کاربر به نرخ داخلی Main->Secondary
            var underlying = ShouldReverseDisplay
                ? decimal.Round(1 / value.Value, 15, MidpointRounding.AwayFromZero)
                : value.Value;

            _ = SetExchangeRateAsync(underlying);
        }
    }

    private async Task SetUnpaidPriceUnitAsync(GetPriceUnitTitleResponse? newUnit)
    {
        if (UnpaidPriceUnit == newUnit) return;

        UnpaidPriceUnit = newUnit;
        await UnpaidPriceUnitChanged.InvokeAsync(newUnit);

        if (newUnit is not null && MainPriceUnit is not null)
        {
            await LoadExchangeRateAsync();
        }
        else
        {
            await SetExchangeRateAsync(null);
        }
    }

    private async Task SetExchangeRateAsync(decimal? newRate)
    {
        if (ExchangeRate == newRate) return;
        await ExchangeRateChanged.InvokeAsync(newRate);
    }

    private async Task LoadExchangeRateAsync()
    {
        if (UnpaidPriceUnit is not null && MainPriceUnit is not null)
        {
            if (UnpaidPriceUnit.Id == MainPriceUnit.Id)
            {
                await SetExchangeRateAsync(null);
                StateHasChanged();
                return;
            }

            decimal? fetchedRate = null;
            await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                action: (s, ct) => s.GetExchangeRateAsync(MainPriceUnit.Id, UnpaidPriceUnit.Id, ct),
                afterSend: response =>
                {
                    fetchedRate = response.ExchangeRate;
                });

            await SetExchangeRateAsync(fetchedRate);
            StateHasChanged();
        }
    }

    private string RateNumericFormat
    {
        get
        {
            // فرمت نمایش مطابق تعداد اعشار گرد شده
            return RateDisplayDecimals switch
            {
                0 => "#,##0",
                1 => "#,##0.0",
                2 => "#,##0.00",
                3 => "#,##0.000",
                4 => "#,##0.0000",
                5 => "#,##0.00000",
                _ => "#,##0.000000"
            };
        }
    }

    private string RateLabel =>
        ShouldReverseDisplay
            ? $"قیمت هر 1 {UnpaidPriceUnit?.Title} به {MainPriceUnit?.Title}"
            : $"نرخ تبدیل 1 {MainPriceUnit?.Title} به {UnpaidPriceUnit?.Title}";

    private string AdornmentText =>
        ShouldReverseDisplay ? MainPriceUnit?.Title ?? "" : UnpaidPriceUnit?.Title ?? "";

    private string HelperText
    {
        get
        {
            if (UnpaidPriceUnit is null || MainPriceUnit is null) return "";
            var disp = RoundedDisplayExchangeRate;
            if (!disp.HasValue) return "";

            if (ShouldReverseDisplay)
                return $"هر 1 {UnpaidPriceUnit.Title} = {disp.Value.ToString(RateNumericFormat)} {MainPriceUnit.Title}";
            else
                return $"هر 1 {MainPriceUnit.Title} = {disp.Value.ToString(RateNumericFormat)} {UnpaidPriceUnit.Title}";
        }
    }

    private bool ShowMainEquivalent => SelectedMode == "custom" && UnpaidPriceUnit is not null;

    private string? SecondaryUnitTitle => UnpaidPriceUnit?.Title;

    private decimal ComputedSecondaryDisplay
    {
        get
        {
            if (ExchangeRate is null || UnpaidPriceUnit is null || MainPriceUnit is null)
                return TotalUnpaidAmount;

            return TotalUnpaidAmount * ExchangeRate.Value;
        }
    }
}