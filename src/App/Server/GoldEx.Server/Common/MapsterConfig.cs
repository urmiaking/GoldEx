using GoldEx.Client.Helpers;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoiceItemAggregate;
using GoldEx.Server.Domain.PaymentMethodAggregate;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Domain.SettingAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PaymentMethods;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;
using Mapster;

namespace GoldEx.Server.Common;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        #region Customers

        config.NewConfig<Customer, GetCustomerResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.CreditLimitPriceUnit, src => src.CreditLimitPriceUnit);

        #endregion

        #region Invoices

        config.NewConfig<Invoice, GetInvoiceListResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.CustomerFullName, src => src.Customer != null ? src.Customer.FullName : string.Empty)
            .Map(dest => dest.AmountUnit, src => src.PriceUnit != null ? src.PriceUnit.Title : string.Empty)
            .Map(dest => dest.PaymentStatus,
                src => Math.Abs(src.TotalUnpaidAmount - 0m) < 0.01m
                    ? InvoicePaymentStatus.Paid
                    : src.DueDate.HasValue && src.DueDate.Value < DateOnly.FromDateTime(DateTime.Today)
                        ? InvoicePaymentStatus.Overdue
                        : InvoicePaymentStatus.HasDebt);

        config.NewConfig<Invoice, GetInvoiceResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Customer, src => src.Customer)
            .Map(dest => dest.InvoiceItems, src => src.Items)
            .Map(dest => dest.InvoiceDiscounts, src => src.Discounts)
            .Map(dest => dest.InvoiceExtraCosts, src => src.ExtraCosts)
            .Map(dest => dest.InvoicePayments, src => src.InvoicePayments)
            .Map(dest => dest.InvoiceDate, 
                src => new DateTime(src.InvoiceDate, TimeOnly.MinValue))
            .Map(dest => dest.DueDate,
                src => src.DueDate.HasValue
                    ? new DateTime(src.DueDate.Value, TimeOnly.MinValue)
                    : (DateTime?)null)
            .Map(dest => dest.PriceUnit, src => src.PriceUnit);

        config.NewConfig<InvoiceItem, GetInvoiceItemResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Product, src => src.Product);

        config.NewConfig<InvoiceDiscount, GetInvoiceDiscountResponse>();

        config.NewConfig<InvoiceExtraCost, GetInvoiceExtraCostsResponse>();

        config.NewConfig<InvoicePayment, GetInvoicePaymentResponse>()
            .Map(dest => dest.PriceUnit, src => src.PriceUnit);

        #endregion

        #region Reporting

        config.NewConfig<Invoice, GetInvoiceDetailResponse>()
            .Map(dest => dest.Customer, src => src.Customer)
            .Map(dest => dest.InvoiceItems, src => src.Items)
            .Map(dest => dest.TaxPercent, src => $"{src.Items.First().TaxPercent.ToCurrencyFormat(null)}%")
            .Map(dest => dest.ProfitPercent, src => $"{src.Items.First().ProfitPercent.ToCurrencyFormat(null)}%")
            .Map(dest => dest.DailyGramPrice, src => $"{src.Items.First().GramPrice.ToCurrencyReportFormat(src.PriceUnit!.Title)}")
            .Map(dest => dest.TotalAmount, src => $"{src.TotalAmount.ToCurrencyReportFormat(src.PriceUnit!.Title)}")
            .Map(dest => dest.TotalPaidAmount, src => $"{src.TotalPaidAmount.ToCurrencyReportFormat(src.PriceUnit!.Title)}")
            .Map(dest => dest.TotalDiscountAmount, src => $"{src.TotalDiscountAmount.ToCurrencyReportFormat(src.PriceUnit!.Title)}")
            .Map(dest => dest.TotalExtraCostAmount, src => $"{src.TotalExtraCostAmount.ToCurrencyReportFormat(src.PriceUnit!.Title)}")
            .Map(dest => dest.TotalUnpaidAmount, src => $"{src.TotalUnpaidAmount.ToCurrencyReportFormat(src.PriceUnit!.Title)}")
            .Map(dest => dest.TotalUnpaidSecondaryAmount,
                src => src.UnpaidPriceUnitId.HasValue
                    ? $"({(src.TotalUnpaidAmount * (src.UnpaidAmountExchangeRate ?? 1)).ToCurrencyReportFormat(src.UnpaidPriceUnit!.Title)})"
                    : null)
            .Map(dest => dest.TotalAmountWithDiscountsAndExtraCosts,
                src => $"{src.TotalAmountWithDiscountsAndExtraCosts.ToCurrencyReportFormat(src.PriceUnit!.Title)}")
            .Map(dest => dest.InvoiceDate,
                src => new DateTime(src.InvoiceDate, TimeOnly.MinValue))
            .Map(dest => dest.DueDate,
                src => src.DueDate.HasValue
                    ? new DateTime(src.DueDate.Value, TimeOnly.MinValue)
                    : (DateTime?)null);

        config.NewConfig<InvoiceItem, GetInvoiceItemReportResponse>()
            .Map(dest => dest.Product, src => src.Product)
            .Map(dest => dest.ItemRawAmount, src => $"{src.ItemRawAmount.ToCurrencyReportFormat(src.PriceUnit!.Title)}")
            .Map(dest => dest.ItemWageAmount, src => $"{src.ItemWageAmount.ToCurrencyReportFormat(src.PriceUnit!.Title)}")
            .Map(dest => dest.ItemProfitAmount, src => $"{src.ItemProfitAmount.ToCurrencyReportFormat(src.PriceUnit!.Title)}")
            .Map(dest => dest.ItemTaxAmount, src => $"{src.ItemTaxAmount.ToCurrencyReportFormat(src.PriceUnit!.Title)}")
            .Map(dest => dest.ItemFinalAmount, src => $"{src.ItemFinalAmount.ToCurrencyReportFormat(src.PriceUnit!.Title)}")
            .Map(dest => dest.TotalAmount, src => $"{src.TotalAmount.ToCurrencyReportFormat(src.PriceUnit!.Title)}")
            .Map(dest => dest.GramPrice, src => $"{src.GramPrice.ToCurrencyReportFormat(src.PriceUnit!.Title)}")
            .Map(dest => dest.ProfitPercent, src => $"{src.ProfitPercent.ToCurrencyReportFormat(null)}%")
            .Map(dest => dest.TaxPercent, src => $"{src.TaxPercent.ToCurrencyReportFormat(null)}%")
            .Map(dest => dest.ExchangeRate,
                src => src.ExchangeRate.HasValue
                    ? $"{src.ExchangeRate.Value.ToCurrencyReportFormat(src.PriceUnit!.Title)}"
                    : null);

        config.NewConfig<Product, GetProductReportResponse>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Barcode, src => src.Barcode)
            .Map(dest => dest.Weight, src => $"{src.Weight.ToWeightFormat()}")
            .Map(dest => dest.Wage, src => src.WageType == WageType.Percent
                ? $"{src.Wage.ToCurrencyReportFormat(null)}%"
                : $"{src.Wage.ToCurrencyReportFormat(src.WagePriceUnit!.Title)}")
            .Map(dest => dest.ProductType, src => src.ProductType)
            .Map(dest => dest.WageType, src => src.WageType)
            .Map(dest => dest.CaratType, src => src.CaratType)
            .Map(dest => dest.ProductCategoryTitle,
                src => src.ProductCategory != null ? src.ProductCategory.Title : string.Empty);

        #endregion

        #region PaymentMethods

        config.NewConfig<PaymentMethod, GetPaymentMethodResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        #endregion

        #region Price

        config.NewConfig<Price, GetPriceResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Value,
                src => src.PriceHistory != null ? src.PriceHistory.CurrentValue.ToString("N0") : "-")
            .Map(dest => dest.Change, src => src.PriceHistory != null ? src.PriceHistory.DailyChangeRate : "-")
            .Map(dest => dest.LastUpdate, src => src.PriceHistory != null ? src.PriceHistory.LastUpdate : "-")
            .Map(dest => dest.Unit, src => src.PriceHistory != null ? src.PriceHistory.Unit : "-")
            .Map(dest => dest.Type, src => src.MarketType)
            .Map(dest => dest.HasIcon,
                src => MapContext.Current.GetService<IWebHostEnvironment>().PriceHistoryIconExists(src.Id.Value));

        config.NewConfig<Price, GetPriceTitleResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Title, src => src.Title);

        #endregion

        #region PriceUnits

        config.NewConfig<PriceUnit, GetPriceUnitResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.PriceId, src => src.PriceId.HasValue ? src.PriceId.Value.Value : (Guid?)null)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.PriceTitle, src => src.Price != null ? src.Price.Title : null)
            .Map(dest => dest.HasIcon,
                src => MapContext.Current.GetService<IWebHostEnvironment>().PriceUnitIconExists(src.Id.Value));

        config.NewConfig<PriceUnit, GetPriceUnitTitleResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.HasIcon,
                src => MapContext.Current.GetService<IWebHostEnvironment>().PriceUnitIconExists(src.Id.Value));

        #endregion

        #region Product

        config.NewConfig<Product, GetProductResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.ProductCategoryId, src => src.ProductCategoryId.HasValue ? src.ProductCategoryId.Value.Value : (Guid?)null)
            .Map(dest => dest.ProductCategoryTitle, src => src.ProductCategory != null ? src.ProductCategory.Title : null)
            .Map(dest => dest.WagePriceUnitId, src => src.WagePriceUnitId.HasValue ? src.WagePriceUnitId.Value.Value : (Guid?)null)
            .Map(dest => dest.WagePriceUnitTitle, src => src.WagePriceUnit != null ? src.WagePriceUnit.Title : null)
            .Map(dest => dest.GemStones, src => src.GemStones);

        config.NewConfig<GemStone, GetGemStoneResponse>();

        #endregion

        #region ProductCategory

        config.NewConfig<ProductCategory, GetProductCategoryResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        #endregion

        #region Settings

        config.NewConfig<Setting, GetSettingResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.HasIcon, src => MapContext.Current.GetService<IWebHostEnvironment>().AppIconExists());

        #endregion

        #region Transactions

        config.NewConfig<Transaction, GetTransactionResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.CreditPriceUnit, src => src.CreditUnit)
            .Map(dest => dest.DebitPriceUnit, src => src.DebitUnit);

        #endregion
    }
}
