using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

public class ReportingMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Invoice, GetInvoiceDetailResponse>()
            .Map(dest => dest.Customer, src => src.Customer)
            .Map(dest => dest.InvoiceItems, src => src.ProductItems)
            .Map(dest => dest.TaxPercent,
                src => src.ProductItems.FirstOrDefault() != null
                    ? $"{src.ProductItems.First().TaxPercent.ToCurrencyFormat(null)}%"
                    : null)
            .Map(dest => dest.ProfitPercent,
                src => src.ProductItems.FirstOrDefault() != null
                    ? $"{src.ProductItems.First().ProfitPercent.ToCurrencyFormat(null)}%"
                    : null)
            .Map(dest => dest.DailyGramPrice,
                src => src.ProductItems.FirstOrDefault() != null
                    ? $"{src.ProductItems.First().GramPrice.ToCurrencyReportFormat(src.PriceUnit == null ? null : src.PriceUnit.Title)}"
                    : null)
            .Map(dest => dest.GoldUnitType,
                src => src.ProductItems.FirstOrDefault() != null
                    ? src.ProductItems.First().Product == null ? GoldUnitType.Gram : src.ProductItems.First().Product!.GoldUnitType
                    : GoldUnitType.Gram)
            .Map(dest => dest.TotalAmount,
                src => $"{src.TotalAmount.ToCurrencyReportFormat(src.PriceUnit == null ? null : src.PriceUnit.Title)}")
            .Map(dest => dest.TotalPaidAmount,
                src => $"{src.TotalPaidAmount.ToCurrencyReportFormat(src.PriceUnit == null ? null : src.PriceUnit.Title)}")
            .Map(dest => dest.TotalDiscountAmount,
                src => $"{src.TotalDiscountAmount.ToCurrencyReportFormat(src.PriceUnit == null ? null : src.PriceUnit.Title)}")
            .Map(dest => dest.TotalExtraCostAmount,
                src => $"{src.TotalExtraCostAmount.ToCurrencyReportFormat(src.PriceUnit == null ? null : src.PriceUnit.Title)}")
            .Map(dest => dest.TotalUnpaidAmount,
                src => $"{src.TotalUnpaidAmount.ToCurrencyReportFormat(src.PriceUnit == null ? null : src.PriceUnit.Title)}")
            .Map(dest => dest.TotalAmountWithDiscountsAndExtraCosts,
                src => $"{src.TotalAmountWithDiscountsAndExtraCosts.ToCurrencyReportFormat(src.PriceUnit == null ? null : src.PriceUnit.Title)}")
            .Map(dest => dest.TotalUnpaidSecondaryAmount,
                src => src.UnpaidPriceUnitId.HasValue
                    ? $"({(src.TotalUnpaidAmount * (src.UnpaidAmountExchangeRate ?? 1)).ToCurrencyReportFormat(src.UnpaidPriceUnit == null ? null : src.UnpaidPriceUnit.Title)})"
                    : null)
            .Map(dest => dest.InvoiceDate,
                src => new DateTime(src.InvoiceDate, TimeOnly.MinValue))
            .Map(dest => dest.DueDate,
                src => src.DueDate.HasValue
                    ? new DateTime(src.DueDate.Value, TimeOnly.MinValue)
                    : (DateTime?)null);

        config.NewConfig<InvoiceProductItem, GetInvoiceItemReportResponse>()
            .Map(dest => dest.Product, src => src.Product)
            .Map(dest => dest.ItemRawAmount,
                src => $"{src.ItemRawAmount.ToCurrencyReportFormat(src.Invoice.PriceUnit!.Title)}")
            .Map(dest => dest.ItemWageAmount,
                src => $"{src.ItemWageAmount.ToCurrencyReportFormat(src.Invoice.PriceUnit!.Title)}")
            .Map(dest => dest.ItemProfitAmount,
                src => $"{src.ItemProfitAmount.ToCurrencyReportFormat(src.Invoice.PriceUnit!.Title)}")
            .Map(dest => dest.ItemTaxAmount,
                src => $"{src.ItemTaxAmount.ToCurrencyReportFormat(src.Invoice.PriceUnit!.Title)}")
            .Map(dest => dest.ItemFinalAmount,
                src => $"{src.ItemFinalAmount.ToCurrencyReportFormat(src.Invoice.PriceUnit!.Title)}")
            .Map(dest => dest.TotalAmount,
                src => $"{src.ItemFinalAmount.ToCurrencyReportFormat(src.Invoice.PriceUnit!.Title)}")
            .Map(dest => dest.GramPrice, src => $"{src.GramPrice.ToCurrencyReportFormat(src.Invoice.PriceUnit!.Title)}")
            .Map(dest => dest.ProfitPercent, src => $"{src.ProfitPercent.ToCurrencyReportFormat(null)}%")
            .Map(dest => dest.TaxPercent, src => $"{src.TaxPercent.ToCurrencyReportFormat(null)}%");

        config.NewConfig<Product, GetProductReportResponse>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Barcode, src => src.Barcode)
            .Map(dest => dest.Weight, src => $"{src.Weight.ToWeightFormat(src.GoldUnitType)}")
            .Map(dest => dest.Wage, src => src.WageType == WageType.Percent
                ? $"{src.Wage.ToCurrencyReportFormat(null)}%"
                : $"{src.Wage.ToCurrencyReportFormat(src.WagePriceUnit!.Title)}")
            .Map(dest => dest.ProductType, src => src.ProductType)
            .Map(dest => dest.WageType, src => src.WageType)
            .Map(dest => dest.Fineness, src => src.Fineness)
            .Map(dest => dest.ProductCategoryTitle,
                src => src.ProductCategory != null ? src.ProductCategory.Title : string.Empty);
    }
}