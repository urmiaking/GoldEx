using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Helpers;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoicePaymentVm
{
    public Guid? Id { get; set; }

    [Display(Name = "تاریخ پرداخت")]
    public DateTime? PaymentDate { get; set; } = DateTime.Now;

    [Display(Name = "کد پیگیری")]
    public string? ReferenceNumber { get; set; }

    [Display(Name = "یادداشت")]
    public string? Note { get; set; }

    [Display(Name = "مبلغ")]
    public decimal Amount { get; set; }

    [Display(Name = "نحوه پرداخت")]
    public PaymentType PaymentType { get; set; }

    public PaymentSide PaymentSide { get; set; }

    [Display(Name = "عیار طلا")]
    public decimal? GoldFineness { get; set; }

    public GetPriceUnitTitleResponse? PriceUnit { get; set; }

    [Display(Name = "حساب مالی")]
    public GetFinancialAccountTitleResponse? FinancialAccount { get; set; }

    [Display(Name = "طرف حساب")]
    public CustomerVm? Endorser { get; set; }

    public List<GetFinancialAccountTitleResponse>? FinancialAccounts { get; set; }

    [Display(Name = "نرخ تبدیل")]
    public decimal? ExchangeRate { get; set; }

    public string? ExchangeRateLabel { get; set; }
    public string AmountAdornmentText { get; set; } = default!;
    public bool AmountMenuOpen { get; set; }
    public bool Disabled { get; set; }

    public Guid? VoucherId { get; set; }

    public decimal FinalAmount => GoldFineness.HasValue ? Amount * GoldFineness.Value / 750m : Amount;

    public string Description
    {
        get
        {
            var typeTitle = PaymentType.GetDisplayTitle();
            var sideTitle = PaymentSide.GetDisplayName();

            var verb = PaymentSide is PaymentSide.Pay ? "از" : "به";

            return PaymentType switch
            {
                PaymentType.InternalCash when FinancialAccount is not null
                    => $"{sideTitle} - {typeTitle} {verb} حساب {FinancialAccount.Title}",

                PaymentType.UsedGoldInventory when GoldFineness.HasValue
                    => $"{sideTitle} - {typeTitle} (عیار {GoldFineness.Value.ToCurrencyFormat()})",

                PaymentType.MoltenGoldInventory when GoldFineness.HasValue
                    => $"{sideTitle} - {typeTitle} (عیار {GoldFineness.Value.ToCurrencyFormat()})",

                PaymentType.CustomerTransfer when Endorser is not null
                    => $"{sideTitle} - {typeTitle} توسط {Endorser.FullName}",

                _ => $"{sideTitle} - {typeTitle}"
            };
        }
    }

    #region ExchangeRate

    private static bool IsMoney(GetPriceUnitTitleResponse? unit)
        => unit is not null && !unit.IsGoldBased;

    private static bool IsGold(GetPriceUnitTitleResponse? unit)
        => unit is not null && unit.IsGoldBased;

    public bool ShouldForceGoldToMoneyDisplay(GetPriceUnitTitleResponse baseUnit)
    {
        if (PriceUnit is null)
            return false;

        return
            (IsGold(PriceUnit) && IsMoney(baseUnit)) ||
            (IsGold(baseUnit) && IsMoney(PriceUnit));
    }

    public bool ShouldReverseExchangeRateDisplay(GetPriceUnitTitleResponse baseUnit)
    {
        if (PriceUnit is null || ExchangeRate is null)
            return false;

        // Money → Money with very small rate
        var isMoneyToMoney =
            !PriceUnit.IsGoldBased &&
            !baseUnit.IsGoldBased &&
            ExchangeRate.Value < 1m;

        // Gold → Money (e.g. gram → toman)
        var isGoldToMoney =
            PriceUnit.IsGoldBased &&
            baseUnit.IsDefault;

        return isMoneyToMoney || isGoldToMoney;
    }

    public decimal? GetDisplayExchangeRate(GetPriceUnitTitleResponse baseUnit)
    {
        if (ExchangeRate is null)
            return null;

        if (!ShouldForceGoldToMoneyDisplay(baseUnit))
            return ExchangeRate;

        // stored is Payment -> Base
        // but UI must show Gold -> Money
        var goldIsPayment = PriceUnit!.IsGoldBased;

        return goldIsPayment
            ? ExchangeRate                         // already Gold -> Money
            : (ExchangeRate == 0 ? null : Math.Round(1 / ExchangeRate.Value, 2));
    }

    public void SetFromDisplayExchangeRate(
        decimal? displayValue,
        GetPriceUnitTitleResponse baseUnit)
    {
        if (!displayValue.HasValue || displayValue <= 0)
            return;

        if (!ShouldForceGoldToMoneyDisplay(baseUnit))
        {
            ExchangeRate = displayValue;
            return;
        }

        var goldIsPayment = PriceUnit!.IsGoldBased;

        ExchangeRate = goldIsPayment
            ? displayValue.Value
            : 1 / displayValue.Value;
    }

    public string GetExchangeRateLabel(GetPriceUnitTitleResponse baseUnit)
    {
        if (PriceUnit is null)
            return string.Empty;

        if (!ShouldForceGoldToMoneyDisplay(baseUnit))
            return $"نرخ تبدیل هر {PriceUnit.Title} به {baseUnit.Title}";

        var gold = PriceUnit.IsGoldBased ? PriceUnit : baseUnit;
        var money = PriceUnit.IsGoldBased ? baseUnit : PriceUnit;

        return $"قیمت هر {gold.Title} به {money.Title}";
    }

    public string GetExchangeRateAdornment(GetPriceUnitTitleResponse baseUnit)
    {
        if (!ShouldForceGoldToMoneyDisplay(baseUnit))
            return baseUnit.Title;

        return PriceUnit!.IsGoldBased
            ? baseUnit.Title      // gold → money
            : PriceUnit.Title;
    }


    #endregion

    public static InvoicePaymentDto ToRequest(InvoicePaymentVm item)
    {
        if (item.PriceUnit is null)
            throw new FluentValidation.ValidationException("واحد ارزی برای پرداختی ها مشخص نشده است");

        if (item.FinancialAccount is null && !item.VoucherId.HasValue && item.PaymentType is PaymentType.InternalCash)
            throw new FluentValidation.ValidationException("حساب مالی برای پرداختی ها مشخص نشده است");

        if (item.PaymentType is PaymentType.CustomerTransfer && item.Endorser?.Id is null)
            throw new FluentValidation.ValidationException("طرف حساب برای حواله کرد مشخص نشده است");

        if (!item.PaymentDate.HasValue)
            throw new FluentValidation.ValidationException("تاریخ پرداخت مشخص نشده است");

        return new InvoicePaymentDto(item.Id,
            item.Amount,
            item.ExchangeRate,
            item.GoldFineness,
            item.PaymentType,
            item.PaymentSide,
            item.PaymentDate.Value,
            item.ReferenceNumber,
            item.Note,
            item.FinancialAccount?.Id,
            item.VoucherId,
            item.Endorser?.Id,
            item.PriceUnit.Id);
    }

    public static InvoicePaymentVm CreateFrom(GetInvoicePaymentResponse response, GetPriceUnitTitleResponse? priceUnit)
    {
        return new InvoicePaymentVm
        {
            Id = response.Id,
            Amount = response.Amount,
            PriceUnit = response.PriceUnit,
            FinancialAccount = response.FinancialAccount,
            PaymentDate = response.PaymentDate,
            ReferenceNumber = response.ReferenceNumber,
            Note = response.Note,
            ExchangeRate = response.ExchangeRate,
            AmountAdornmentText = response.PriceUnit.Title,
            ExchangeRateLabel = response.PriceUnit != priceUnit ? $"نرخ تبدیل {response.PriceUnit.Title} به {priceUnit?.Title}" : null,
            VoucherId = response.VoucherId,
            Disabled = response.VoucherId.HasValue,
            FinancialAccounts = response.FinancialAccounts,
            Endorser = response.Endorser != null ? CustomerVm.CreateFrom(response.Endorser) : null,
            PaymentType = response.PaymentType,
            PaymentSide = response.PaymentSide,
            GoldFineness = response.GoldFineness
        };
    }
}