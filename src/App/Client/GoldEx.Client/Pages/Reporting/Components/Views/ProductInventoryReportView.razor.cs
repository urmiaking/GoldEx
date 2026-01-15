using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using static GoldEx.Client.Pages.Reporting.ViewModels.ProductInventoryFilterVm;

namespace GoldEx.Client.Pages.Reporting.Components.Views;

public partial class ProductInventoryReportView
{
    [Parameter] public List<ProductInventoryRpResponse>? Items { get; set; }
    [Parameter] public EventCallback OnPrintReport { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private ProductInventoryReportSummary? _summary;

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

        var totalCurrentAmount = Items.Sum(x => x.CurrentAmount);
        var totalSoldAmount = Items.Sum(x => x.SoldAmount);

        _summary = new ProductInventoryReportSummary
        {
            TotalAmount = totalCurrentAmount,
            TotalSoldAmount = totalSoldAmount,
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