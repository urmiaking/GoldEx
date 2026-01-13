using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Domain.FinancialAccountAggregate.Extensions;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;

namespace GoldEx.Server.Application.Utilities;

public static class InventoryStockDescriptionBuilder
{
    public static string Build(InventoryStock src, bool includeExtraInfo)
    {
        var prefix = src.ReverseInventoryStockId.HasValue ? "برگشت: " : string.Empty;
        var action = src.ActionType == WarehouseActionType.In ? "ورود" : "خروج";
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
            else if (src.InventoryEntryId.HasValue)
            {
                extraInfo = "طبق ثبت موجودی اولیه";
            }
            else if (src.Transactions != null && src.Transactions.Any())
            {
                extraInfo = $"طبق تراکنش : {src.Transactions.First().Description}";
            }

        }

        if (src.Product != null)
        {
            return $"{prefix}{action} {src.ChangeAmount.ToWeightFormat(src.Product.GoldUnitType)} {src.Product.Name} {extraInfo}";
        }

        if (src.CoinInstance != null)
        {
            return $"{prefix}{action} {src.ChangeAmount:G29} عدد {src.CoinInstance.Coin?.Title} {extraInfo}";
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

    public static string BuildUrl(InventoryStock src)
    {
        if (src.MeltingBatchId.HasValue)
        {
            return ClientRoutes.InventoryStocks.MeltingBatches.Index.AppendQueryString(new { q = src.MeltingBatchId.Value.Value });
        }
        if (src.InvoiceId.HasValue)
        {
            return ClientRoutes.Invoices.SetInvoice.FormatRoute(new { id = src.InvoiceId.Value.Value });
        }
        if (src.InventoryEntryId.HasValue)
        {
            return ClientRoutes.InventoryStocks.InventoryEntry.List.AppendQueryString(new { q = src.InventoryEntryId.Value.Value });
        }

        return string.Empty;
    }
}