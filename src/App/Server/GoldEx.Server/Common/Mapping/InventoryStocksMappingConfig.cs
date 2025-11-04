using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Domain.FinancialAccountAggregate.Extensions;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Infrastructure.Models;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

public class InventoryStocksMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<InventoryStock, GetInventoryStockResponse>()
            .Map(dest => dest.CurrentAmount, src => src.ChangeAmount);

        config.NewConfig<InventorySummaryData, GetInventoryStockResponse>()
            .Map(dest => dest.CurrentAmount, src => src.CurrentAmount)
            .Map(dest => dest.SoldAmount, src => src.SoldAmount)
            .Map(dest => dest.DateTime, src => src.DateTime);

        config.NewConfig<InventoryWeightChartData, GetInventoryWeightChartResponse>();

        config.NewConfig<InventoryStock, GetInventoryStockItemResponse>()
            .Map(dest => dest.Amount, src => src.ChangeAmount)
            .Map(dest => dest.DateTime, src => src.PostingDate)
            .Map(dest => dest.ActionType, src => src.ActionType)
            .Map(dest => dest.GoldUnitType, src =>
                src.Product != null ? src.Product.GoldUnitType : (GoldUnitType?)null)
            .Map(dest => dest.PriceUnit, src => src.Currency != null ? src.Currency.Title : null)
            .Map(dest => dest.Description, src => GenerateDescription(src, false));

        config.NewConfig<InventoryStock, GetInventoryStockTraceResponse>()
            .Map(dest => dest.Amount, src => src.ChangeAmount)
            .Map(dest => dest.DateTime, src => src.PostingDate)
            .Map(dest => dest.ActionType, src => src.ActionType)
            .Map(dest => dest.GoldUnitType, src =>
                src.Product != null ? src.Product.GoldUnitType : (GoldUnitType?)null)
            .Map(dest => dest.PriceUnit, src => src.Currency != null ? src.Currency.Title : null)
            .Map(dest => dest.Description, src => GenerateDescription(src, true))
            .Map(dest => dest.SourceUrl, src => GenerateSourceUrl(src));
    }

    private static string GenerateDescription(InventoryStock src, bool includeExtraInfo)
    {
        var prefix = src.ReverseInventoryStockId.HasValue ? "برگشت: " : string.Empty;
        var action = src.ActionType == WarehouseActionType.In ? "خرید" : "فروش";
        var hyphen = src.ActionType == WarehouseActionType.In ? "از" : "به";

        string? extraInfo = null;

        if (includeExtraInfo)
        {
            if (src.Invoice is not null)
            {
                extraInfo = $"طبق فاکتور {src.Invoice.InvoiceType.GetDisplayName()} شماره {src.Invoice.InvoiceNumber}";

                if (src.Invoice.Customer is not null)
                {
                    extraInfo += $" {hyphen} {src.Invoice.Customer.FullName}";
                }
            }
            else if (src.MeltingBatch is not null)
            {
                extraInfo = $"طبق ذوب شماره {src.MeltingBatch.BatchNumber}";
            }
        }

        if (src.Product != null)
        {
            return $"{prefix}{action} {src.ChangeAmount.ToWeightFormat(src.Product.GoldUnitType)} {src.Product.Name} {extraInfo}";
        }

        if (src.Coin != null)
        {
            return $"{prefix}{action} {src.ChangeAmount:G29} عدد {src.Coin.Title} {extraInfo}";
        }

        if (src.Currency != null)
        {
            var currencyDescription = $"{prefix}{action} {src.ChangeAmount.ToCurrencyFormat(src.Currency.Title)}";

            // اضافه کردن اطلاعات حساب مالی فقط برای ارز
            var currencyItem = src.Invoice?.CurrencyItems
                .FirstOrDefault(ci => ci.CurrencyId == src.CurrencyId);

            if (currencyItem?.FinancialAccount is not null)
            {
                var accountAction = src.ActionType == WarehouseActionType.In ? "واریز به" : "برداشت از";
                var accountText = currencyItem.FinancialAccount.GetAccountDisplayText();
                currencyDescription += $" - {accountAction} {accountText}";
            }

            return $"{currencyDescription} {extraInfo}";
        }

        return $"{prefix}{action} {src.ChangeAmount:G29} واحد نامشخص {extraInfo}";
    }

    private static string GenerateSourceUrl(InventoryStock src)
    {
        if (src.MeltingBatchId.HasValue)
        {
            // TODO: Update this when melting batch detail page is created
            return ClientRoutes.InventoryStocks.MeltingBatches.Index;
        }
        if (src.InvoiceId.HasValue)
        {
            return ClientRoutes.Invoices.SetInvoice.FormatRoute(new { id = src.InvoiceId.Value.Value });
        }

        return string.Empty;
    }
}