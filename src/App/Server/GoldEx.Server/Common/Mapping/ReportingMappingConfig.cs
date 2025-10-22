using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
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
            .Map(dest => dest.InvoiceProductItems, src => src.ProductItems)
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
                    ? $"{src.ProductItems.First().GramPrice.ToCurrencyReportFormat(src.PriceUnit!.Title)}" +
                      (src.ExchangeRate.HasValue && src.BasePriceUnit != null
                          ? $" (معادل {src.ExchangeRate.Value.ToCurrencyReportFormat(null)} {src.BasePriceUnit.Title})"
                          : null)
                    : null)
            .Map(dest => dest.GoldUnitType,
                src => src.ProductItems.FirstOrDefault() != null
                    ? src.ProductItems.First().Product == null
                        ? GoldUnitType.Gram.GetDisplayName()
                        : src.ProductItems.First().Product!.GoldUnitType.GetDisplayName()
                    : GoldUnitType.Gram.GetDisplayName())
            .Map(dest => dest.TotalAmount,
                src => $"{src.TotalAmount.ToCurrencyReportFormat(src.PriceUnit == null ? null : src.PriceUnit.Title)}")
            .Map(dest => dest.TotalPaidAmount,
                src =>
                    $"{src.TotalPaidAmount.ToCurrencyReportFormat(src.PriceUnit == null ? null : src.PriceUnit.Title)}")
            .Map(dest => dest.TotalDiscountAmount,
                src => src.TotalDiscountAmount != 0
                    ? $"{src.TotalDiscountAmount.ToCurrencyReportFormat(src.PriceUnit == null ? null : src.PriceUnit.Title)}"
                    : null)
            .Map(dest => dest.TotalExtraCostAmount,
                src => src.TotalExtraCostAmount != 0
                    ? $"{src.TotalExtraCostAmount.ToCurrencyReportFormat(src.PriceUnit == null ? null : src.PriceUnit.Title)}"
                    : null)
            .Map(dest => dest.TotalUnpaidAmount,
                src =>
                    $"{(src.TotalUnpaidAmount).FormatUnpaidAmount(src.PriceUnit == null ? null : src.PriceUnit.Title)}")
            .Map(dest => dest.TotalPayableAmount,
                src =>
                    $"{(src.TotalAmountWithDiscountsAndExtraCosts - src.TotalUsedProductsAmount).ToCurrencyReportFormat(src.PriceUnit == null ? null : src.PriceUnit.Title)}")
            .Map(dest => dest.TotalUnpaidSecondaryAmount,
                src => src.UnpaidPriceUnitId.HasValue
                    ? $"({(src.TotalUnpaidAmount * (src.UnpaidAmountExchangeRate ?? 1))
                        .ToCurrencyReportFormat(src.UnpaidPriceUnit == null ? null : src.UnpaidPriceUnit.Title)})"
                    : null)
            .Map(dest => dest.TotalStoneAmount, src => src.TotalStoneAmount != 0
                ? $"{src.TotalStoneAmount.ToCurrencyReportFormat(src.PriceUnit == null ? null : src.PriceUnit.Title)}"
                : null)
            .Map(dest => dest.InvoiceDate,
                src => new DateTime(src.InvoiceDate, TimeOnly.MinValue))
            .Map(dest => dest.DueDate,
                src => src.DueDate.HasValue
                    ? new DateTime(src.DueDate.Value, TimeOnly.MinValue)
                    : (DateTime?)null)
            .Map(dest => dest.TotalUsedProductsAmount, src => src.TotalUsedProductsAmount != 0
                ? $"{src.TotalUsedProductsAmount.ToCurrencyReportFormat(src.PriceUnit!.Title)}"
                : null)
            .Map(dest => dest.InvoiceProductItems, src => src.ProductItems)
            .Map(dest => dest.InvoiceCoinItems, src => src.CoinItems)
            .Map(dest => dest.InvoiceCurrencyItems, src => src.CurrencyItems)
            .Map(dest => dest.InvoiceUsedProductItems, src => src.UsedProducts)
            .Map(dest => dest.InvoicePayments, src => src.InvoicePayments);

        config.NewConfig<InvoiceProductItem, GetInvoiceProductItemReportResponse>()
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
            .Map(dest => dest.TotalWeight, 
                src => $"{src.TotalWeight.ToWeightFormat(src.Product != null ? src.Product.GoldUnitType : GoldUnitType.Gram)}")
            .Map(dest => dest.GramPrice, src =>
                $"{src.GramPrice.ToCurrencyReportFormat(src.Invoice.PriceUnit!.Title)}")
            .Map(dest => dest.ProfitPercent, src =>
                $"{src.ProfitPercent.ToCurrencyReportFormat(null)}%")
            .Map(dest => dest.TaxPercent, src =>
                $"{src.TaxPercent.ToCurrencyReportFormat(null)}%")
            .Map(dest => dest.SaleWageType, src => src.SaleWageType)
            .Map(dest => dest.SaleWage, src =>
                src.SaleWage != null
                    ? src.SaleWageType != null
                        ? src.SaleWageType == WageType.Percent
                            ? $"{src.SaleWage.Value.ToCurrencyReportFormat(null)}%"
                            : $"{src.SaleWage.Value.ToCurrencyReportFormat(src.SaleWagePriceUnit!.Title)}"
                        : "ندارد"
                    : src.Product!.WageType != null
                        ? src.Product!.WageType == WageType.Percent
                            ? $"{src.Product!.Wage.ToCurrencyReportFormat(null)}%"
                            : $"{src.Product!.Wage.ToCurrencyReportFormat(src.Product!.WagePriceUnit!.Title)}"
                        : "ندارد");

        config.NewConfig<Product, GetProductReportResponse>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Barcode, src => src.Barcode)
            .Map(dest => dest.Weight, src => $"{src.Weight.ToWeightFormat(src.GoldUnitType)}")
            .Map(dest => dest.Wage, src => src.WageType != null ? src.WageType == WageType.Percent
                ? $"{src.Wage.ToCurrencyReportFormat(null)}%"
                : $"{src.Wage.ToCurrencyReportFormat(src.WagePriceUnit!.Title)}" : "ندارد")
            .Map(dest => dest.ProductType, src => src.ProductType)
            .Map(dest => dest.WageType, src => src.WageType)
            .Map(dest => dest.Fineness, src => src.Fineness)
            .Map(dest => dest.ProductCategoryTitle,
                src => src.ProductCategory != null ? src.ProductCategory.Title : string.Empty);

        config.NewConfig<InvoiceCoinItem, GetInvoiceCoinItemReportResponse>()
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.TotalPrice, src => 
                src.ItemFinalAmount.ToCurrencyReportFormat(src.Invoice.PriceUnit!.Title))
            .Map(dest => dest.CoinTitle, src => 
                src.Coin != null ? src.Coin.Title : string.Empty);

        config.NewConfig<InvoiceCurrencyItem, GetInvoiceCurrencyItemReportResponse>()
            .Map(dest => dest.Amount, src => src.Amount.ToCurrencyReportFormat(null))
            .Map(dest => dest.TotalPrice, src =>
                src.ItemFinalAmount.ToCurrencyReportFormat(src.Invoice.PriceUnit!.Title))
            .Map(dest => dest.CurrencyTitle, src =>
                src.Currency != null ? src.Currency.Title : string.Empty);

        config.NewConfig<InvoiceUsedProduct, GetInvoiceUsedProductReportResponse>()
            .Map(dest => dest.Title, src => src.Description)
            .Map(dest => dest.ExtraCosts,
                src => src.ExtraCostsAmount.HasValue
                    ? src.ExtraCostsAmount.Value.ToCurrencyReportFormat(src.Invoice.PriceUnit!.Title)
                    : null)
            .Map(dest => dest.Fineness, src => src.FinenessDeductionRate)
            .Map(dest => dest.GoldUnitType, src => src.UnitType)
            .Map(dest => dest.Weight, src => src.Weight.ToWeightFormat(src.UnitType))
            .Map(dest => dest.TotalPrice, src =>
                src.ItemFinalAmount.ToCurrencyReportFormat(src.Invoice.PriceUnit!.Title));

        config.NewConfig<InvoicePayment, GetInvoicePaymentReportResponse>()
            .Map(dest => dest.FinalAmount, src => 
                src.PriceUnit != null ? src.FinalAmount.ToCurrencyReportFormat(src.PriceUnit.Title) : string.Empty)
            .Map(dest => dest.PaymentType, src => src.PaymentType)
            .Map(dest => dest.PaymentDate, src => src.PaymentDate)
            .Map(dest => dest.Description, src => PaymentDescriptionBuilder.Build(src));
    }
}