using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.CoinInstances;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace GoldEx.Client.Pages.Reporting.Pages.Prints;

public partial class CoinInventoryReportPrint
{
    [Parameter, SupplyParameterFromQuery]
    public Guid? CoinId { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public string? ItemStatus { get; set; }

    public string TitleFilter
    {
        get
        {
            string? output = null;

            if (_coin is not null)
            {
                output += $" ({_coin.Title})";
            }

            if (_itemStatus.HasValue)
            {
                output += $" ({_itemStatus.Value.GetDisplayName()})";
            }

            return output ?? string.Empty;
        }
    }

    private readonly int _version = new Random().Next(0, 1000);
    private CoinInventoryFilterVm _filter = default!;
    private GetCoinResponse? _coin;
    private List<CoinInventoryRpResponse>? _items;
    private ReportSummaryVm? _summary;
    private ItemStatus? _itemStatus;

    protected override void OnInitialized()
    {
        if (!string.IsNullOrWhiteSpace(ItemStatus) &&
            Enum.TryParse<ItemStatus>(ItemStatus, ignoreCase: true, out var parsedStatus))
        {
            _itemStatus = parsedStatus;
        }

        _filter = new CoinInventoryFilterVm
        {
            CoinId = CoinId,
            ItemStatus = _itemStatus,
            DateRange = DateRangeExtensions.From(FromDate, ToDate),
        };
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadProductCategoryAsync();
        await LoadReportAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadProductCategoryAsync()
    {
        if (!CoinId.HasValue)
            return;

        _coin = await SendRequestAsync<ICoinService, GetCoinResponse>(
            action: (s, ct) => s.GetAsync(CoinId.Value, ct),
            createScope: true);
    }

    private async Task LoadReportAsync()
    {
        var request = _filter.ToRequest();

        _items = await SendRequestAsync<IReportingService, List<CoinInventoryRpResponse>>(
            action: (s, ct) => s.GetCoinInventoryAsync(request, ct),
            createScope: true);

        CalculateSummary();
    }

    private void CalculateSummary()
    {
        if (_items == null || !_items.Any())
        {
            _summary = null;
            return;
        }

        var totalAvailableAmount = _items.Sum(x => x.CurrentAmount);
        var totalSoldAmount = _items.Sum(x => x.SoldAmount);
        var totalAmount = totalAvailableAmount + totalSoldAmount;

        _summary = new ReportSummaryVm
        {
            Sections =
            [
                new SummarySection
                {
                    Items =
                    [
                        new SummaryItem
                        {
                            Label = "موجود",
                            Value = totalAvailableAmount.ToString("G29"),
                            Type = "credit",
                            ShowIcon = true,
                            IconType = "credit-icon"
                        },
                        new SummaryItem
                        {
                            Label = "فروخته شده",
                            Value = totalSoldAmount.ToString("G29"),
                            Type = "debit",
                            ShowIcon = true,
                            IconType = "debit-icon"
                        },
                        new SummaryItem
                        {
                            Label = "تعداد کل",
                            Value = Math.Max(0, totalAmount).ToString("G29"),
                            Type = "net"
                        }
                    ]
                }
            ]
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