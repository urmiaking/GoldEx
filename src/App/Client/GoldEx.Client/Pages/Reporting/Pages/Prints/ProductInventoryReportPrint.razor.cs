using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Reporting.Pages.Prints;

public partial class ProductInventoryReportPrint
{
    [Parameter, SupplyParameterFromQuery] 
    public Guid? ProductCategoryId { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public string? ItemType { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public string? ItemStatus { get; set; }

    public string TitleFilter
    {
        get
        {
            string? output = null;
            if (_itemType.HasValue)
            {
                output += $" ({_itemType.Value.GetDisplayName()})";
            }

            if (_productCategory is not null)
            {
                output += $" دسته {_productCategory.Title}";
            }

            if (_itemStatus.HasValue)
            {
                output += $" ({_itemStatus.Value.GetDisplayName()})";
            }

            return output ?? string.Empty;
        }
    }

    private readonly int _version = new Random().Next(0, 1000);
    private ProductInventoryFilterVm _filter = default!;
    private GetProductCategoryResponse? _productCategory;
    private List<ProductInventoryRpResponse>? _items;
    private ReportSummaryVm? _summary;
    private ItemType? _itemType;
    private ItemStatus? _itemStatus;

    protected override void OnInitialized()
    {
        if (!string.IsNullOrWhiteSpace(ItemType) &&
            Enum.TryParse<ItemType>(ItemType, ignoreCase: true, out var parsedType))
        {
            _itemType = parsedType;
        }

        if (!string.IsNullOrWhiteSpace(ItemStatus) &&
            Enum.TryParse<ItemStatus>(ItemStatus, ignoreCase: true, out var parsedStatus))
        {
            _itemStatus = parsedStatus;
        }

        _filter = new ProductInventoryFilterVm
        {
            ProductCategoryId = ProductCategoryId,
            ItemType = _itemType,
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
        if (!ProductCategoryId.HasValue)
            return;

        _productCategory = await SendRequestAsync<IProductCategoryService, GetProductCategoryResponse>(
            action: (s, ct) => s.GetAsync(ProductCategoryId.Value, ct),
            createScope: true);
    }

    private async Task LoadReportAsync()
    {
        var request = _filter.ToRequest();

        _items = await SendRequestAsync<IReportingService, List<ProductInventoryRpResponse>>(
            action: (s, ct) => s.GetProductInventoryAsync(request, ct),
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

        var totalCurrentAmount = _items.Sum(x => x.CurrentAmount);
        var totalSoldAmount = _items.Sum(x => x.SoldAmount);
        var totalRemaining = totalCurrentAmount - totalSoldAmount;

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
                            Label = "موجودی کل",
                            Value = totalCurrentAmount.ToWeightFormat(GoldUnitType.Gram),
                            Type = "credit",
                            ShowIcon = true,
                            IconType = "credit-icon"
                        },
                        new SummaryItem
                        {
                            Label = "موجودی فروخته شده",
                            Value = totalSoldAmount.ToWeightFormat(GoldUnitType.Gram),
                            Type = "debit",
                            ShowIcon = true,
                            IconType = "debit-icon"
                        },
                        new SummaryItem
                        {
                            Label = "موجودی فعلی",
                            Value = Math.Max(0, totalRemaining).ToWeightFormat(GoldUnitType.Gram),
                            Type = "net",
                        }
                    ]
                }
            ]
        };
    }

    private string GetItemWage(ProductInventoryRpResponse context)
    {
        if (context.CurrentAmount == 0)
        {
            return context.SaleWageType switch
            {
                WageType.Fixed => $"{context.SaleWage?.ToCurrencyFormat(context.SaleWagePriceUnitTitle)}",
                WageType.Percent => context.SaleWage?.ToString("G29") + " درصد",
                _ => "ندارد"
            };
        }
        else
        {
            return context.Product.WageType switch
            {
                WageType.Fixed => $"{context.Product.Wage?.ToCurrencyFormat(context.Product.WagePriceUnitTitle)}",
                WageType.Percent => context.Product.Wage?.ToString("G29") + " درصد",
                _ => "ندارد"
            };
        }
    }
}