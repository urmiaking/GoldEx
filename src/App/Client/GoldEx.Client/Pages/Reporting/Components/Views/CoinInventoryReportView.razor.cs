using GoldEx.Shared.DTOs.CoinInstances;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using static GoldEx.Client.Pages.Reporting.ViewModels.CoinInventoryFilterVm;

namespace GoldEx.Client.Pages.Reporting.Components.Views;

public partial class CoinInventoryReportView
{
    [Parameter] public List<CoinInventoryRpResponse>? Items { get; set; }
    [Parameter] public EventCallback OnPrintReport { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private CoinInventoryReportSummary? _summary;

    protected override void OnParametersSet()
    {
        CalculateSummary();
        base.OnParametersSet();
    }

    private void CalculateSummary()
    {
        if (Items == null || Items.Count == 0)
        {
            _summary = null;
            return;
        }

        var totalAvailableAmount = Items.Sum(x => x.CurrentAmount);
        var totalSoldAmount = Items.Sum(x => x.SoldAmount);

        _summary = new CoinInventoryReportSummary
        {
            TotalAvailableAmount = totalAvailableAmount,
            TotalSoldAmount = totalSoldAmount
        };
    }

    private string GetCoinTitle(GetCoinInstanceResponse coinInstance)
    {
        var coin = coinInstance.Coin;

        var baseTitle = coin.Title;

        var issuer = coinInstance.CoinPackage?.Issuer;
        if (issuer is null)
            return baseTitle;

        var issuerName = issuer.FullName;
        var nationalCode = issuer.NationalId;

        if (string.IsNullOrWhiteSpace(issuerName) || string.IsNullOrWhiteSpace(nationalCode))
            return baseTitle;

        return $"{baseTitle} - {issuerName} ({nationalCode})";
    }

    private string GetCoinWeight(GetCoinInstanceResponse coinInstance)
    {
        var weight = coinInstance.Weight.ToWeightFormat(GoldUnitType.Gram);

        var vacuumedWeight = coinInstance.CoinPackage?.VacuumedWeight.ToWeightFormat(GoldUnitType.Gram);

        return vacuumedWeight is not null
            ? $"{weight} ({vacuumedWeight} با پرس)"
            : weight;
    }

    private string GetCoinMintYear(GetCoinInstanceResponse coinItem)
    {
        DateTime? mintDate = coinItem.MintYear.HasValue 
            ? new DateTime(coinItem.MintYear.Value, 3, 21) 
            : null;

        if (!mintDate.HasValue)
            return "نامشخص";

        var persianYear = new PersianCalendar().GetYear(mintDate.Value);

        return persianYear.ToString();
    }
}