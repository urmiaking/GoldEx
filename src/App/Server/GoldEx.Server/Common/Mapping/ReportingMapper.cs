using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Infrastructure.Models;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class ReportingMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LedgerAccountTrialBalanceNodeModel, LedgerAccountTrialBalanceRpResponse>();

        config.NewConfig<Invoice, SellInvoiceRpResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.PriceUnit, src => src.PriceUnit != null ? src.PriceUnit.Title : string.Empty)
            .Map(dest => dest.CustomerName, src => src.Customer!.FullName)
            .Map(dest => dest.RemainingPrice, src => src.TotalUnpaidAmount)
            .Map(dest => dest.TotalPrice, src => src.TotalAmountWithDiscountsAndExtraCosts)
            .Map(dest => dest.TotalProfit, src => src.TotalProfitAmount)
            .Map(dest => dest.TotalTax, src => src.TotalTaxAmount)
            .Map(dest => dest.TotalWage, src => src.TotalWageAmount)
            .Map(dest => dest.TotalWeightEquivalent, src => GoldWeightCalculator.CalculateTotalWeight(src))
            .Map(dest => dest.RemainingWeightEquivalent, src => GoldWeightCalculator.CalculateRemainingWeight(src))
            .Map(dest => dest.ProfitWeightEquivalent, src => GoldWeightCalculator.CalculateProfitWeight(src))
            .Map(dest => dest.WageWeightEquivalent, src => GoldWeightCalculator.CalculateWageWeight(src))
            .Map(dest => dest.TaxWeightEquivalent, src => GoldWeightCalculator.CalculateTaxWeight(src));

        config.NewConfig<Invoice, PurchaseInvoiceRpResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.CustomerName, src => src.Customer!.FullName)
            .Map(dest => dest.PriceUnit, src => src.PriceUnit != null ? src.PriceUnit.Title : string.Empty)
            .Map(dest => dest.RemainingPrice, src => src.TotalUnpaidAmount)
            .Map(dest => dest.TotalPrice, src => src.TotalAmountWithDiscountsAndExtraCosts)
            .Map(dest => dest.TotalWeightEquivalent, src => GoldWeightCalculator.CalculateTotalWeight(src))
            .Map(dest => dest.RemainingWeightEquivalent, src => GoldWeightCalculator.CalculateRemainingWeight(src));

        config.NewConfig<InvoicePayment, PaymentRpResponse>()
            .Map(dest => dest.CustomerName, src => src.Invoice!.Customer!.FullName)
            .Map(dest => dest.InvoiceId, src => src.InvoiceId.Value)
            .Map(dest => dest.InvoiceNumber, src => src.Invoice!.InvoiceNumber)
            .Map(dest => dest.InvoiceType, src => src.Invoice!.InvoiceType)
            .Map(dest => dest.PriceUnit, src => src.PriceUnit!.Title)
            .Map(dest => dest.Description, src => PaymentDescriptionBuilder.Build(src, true));
        
        config.NewConfig<InvoicePayment, InvoicePaymentRpResponse>()
            .Map(dest => dest.InvoiceId, src => src.InvoiceId.Value)
            .Map(dest => dest.CustomerName, src => src.Invoice!.Customer!.FullName)
            .Map(dest => dest.InvoiceRemainingPrice, src => src.Invoice!.TotalUnpaidAmount)
            .Map(dest => dest.InvoicePriceUnit, src => src.Invoice!.PriceUnit!.Title)
            .Map(dest => dest.PriceUnit, src => src.PriceUnit!.Title)
            .Map(dest => dest.Description, src => PaymentDescriptionBuilder.Build(src, true));

        config.NewConfig<InventoryStock, InventoryKardexRpResponse>()
            .Map(dest => dest.Amount, src => src.ChangeAmount)
            .Map(dest => dest.DateTime, src => src.PostingDate)
            .Map(dest => dest.ActionType, src => src.ActionType)
            .Map(dest => dest.GoldUnitType, src =>
                src.Product != null ? src.Product.GoldUnitType : (GoldUnitType?)null)
            .Map(dest => dest.PriceUnit, src => src.Currency != null ? src.Currency.Title : null)
            .Map(dest => dest.Description, src => InventoryStockDescriptionBuilder.Build(src, true))
            .Map(dest => dest.SourceUrl, src => InventoryStockDescriptionBuilder.BuildUrl(src));

        config.NewConfig<InventorySummaryData, ProductInventoryRpResponse>();

        config.NewConfig<InventorySummaryData, CoinInventoryRpResponse>();

        config.NewConfig<InventorySummaryData, CurrencyInventoryRpResponse>();
    }
}

internal static class GoldWeightCalculator
{
    public static decimal CalculateTotalWeight(Invoice src)
    {
        if (src.PriceUnit != null && src.PriceUnit.IsGoldBased)
            return src.TotalAmountWithDiscountsAndExtraCosts;

        var (totalItemWeight, totalItemFinalAmount) = GetProductItemTotals(src);
        if (totalItemWeight == 0) return 0;

        decimal effectiveRate = totalItemFinalAmount / totalItemWeight;
        if (effectiveRate == 0) return totalItemWeight;

        var discountWeight = src.TotalDiscountAmount / effectiveRate;
        var extraCostWeight = src.TotalExtraCostAmount / effectiveRate;
        return totalItemWeight - discountWeight + extraCostWeight;
    }

    public static decimal CalculateRemainingWeight(Invoice src)
    {
        if (src.PriceUnit != null && src.PriceUnit.IsGoldBased)
            return src.TotalUnpaidAmount;

        var (totalItemWeight, totalItemFinalAmount) = GetProductItemTotals(src);
        if (totalItemWeight == 0) return 0;

        decimal effectiveRate = totalItemFinalAmount / totalItemWeight;
        if (effectiveRate == 0) return 0;

        return src.TotalUnpaidAmount / effectiveRate;
    }

    public static decimal CalculateProfitWeight(Invoice src)
    {
        if (src.PriceUnit != null && src.PriceUnit.IsGoldBased)
            return src.TotalProfitAmount;

        if (src.ProductItems == null) return 0;

        decimal profitWeight = 0;
        foreach (var item in src.ProductItems)
        {
            if (item.GramPrice > 0)
            {
                profitWeight += item.ItemProfitAmount / item.GramPrice;
            }
        }
        return profitWeight;
    }

    public static decimal CalculateWageWeight(Invoice src)
    {
        if (src.PriceUnit != null && src.PriceUnit.IsGoldBased)
            return src.TotalWageAmount;

        if (src.ProductItems == null) return 0;

        decimal wageWeight = 0;
        foreach (var item in src.ProductItems)
        {
            if (item.GramPrice > 0)
            {
                wageWeight += item.ItemWageAmount / item.GramPrice;
            }
        }
        return wageWeight;
    }

    public static decimal CalculateTaxWeight(Invoice src)
    {
        if (src.PriceUnit != null && src.PriceUnit.IsGoldBased)
            return src.TotalTaxAmount;

        if (src.ProductItems == null) return 0;

        decimal taxWeight = 0;
        foreach (var item in src.ProductItems)
        {
            if (item.GramPrice > 0)
            {
                taxWeight += item.ItemTaxAmount / item.GramPrice;
            }
        }
        return taxWeight;
    }

    private static (decimal TotalWeight, decimal TotalFinalAmount) GetProductItemTotals(Invoice src)
    {
        decimal totalWeight = 0;
        decimal totalFinalAmount = 0;

        if (src.ProductItems != null)
        {
            foreach (var item in src.ProductItems)
            {
                if (item.GramPrice > 0)
                {
                    totalWeight += item.ItemFinalAmount / item.GramPrice;
                    totalFinalAmount += item.ItemFinalAmount;
                }
            }
        }

        if (src.UsedProducts != null)
        {
            foreach (var item in src.UsedProducts)
            {
                if (item.GramPrice > 0)
                {
                    totalWeight += item.ItemFinalAmount / item.GramPrice;
                    totalFinalAmount += item.ItemFinalAmount;
                }
                else if (item.Weight > 0)
                {
                    var fineness = item.Product?.Fineness ?? 750m;
                    var standardWeight = fineness > 0 ? (item.Weight * fineness / 750m) : item.Weight;
                    totalWeight += standardWeight;
                    totalFinalAmount += item.ItemFinalAmount;
                }
            }
        }

        return (totalWeight, totalFinalAmount);
    }
}