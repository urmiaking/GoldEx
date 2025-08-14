using GoldEx.Client.Helpers;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Domain.SettingAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.LedgerAccounts;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using Mapster;

namespace GoldEx.Server.Common;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        #region Coins

        config.NewConfig<Coin, GetCoinResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.PriceId, src => src.PriceId.HasValue ? src.PriceId.Value.Value : (Guid?)null);

        #endregion

        #region Customers

        config.NewConfig<Customer, GetCustomerResponse>()
            .PreserveReference(true)
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.CreditLimitPriceUnit, src => src.CreditLimitPriceUnit)
            .Map(dest => dest.FinancialAccounts, src => src.FinancialAccounts);

        #endregion

        #region FinancialAccounts

        config.NewConfig<FinancialAccount, GetFinancialAccountResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.FinancialAccountType, src => src.AccountType)
            .Map(dest => dest.SupplierFullName, src =>
                src.Customer != null ? src.Customer.FullName : string.Empty)
            .Map(dest => dest.SupplierPhoneNumber, src =>
                src.Customer != null ? src.Customer.PhoneNumber : string.Empty)
            .Map(dest => dest.PriceUnit, src => src.PriceUnit)
            .Map(dest => dest.LocalBankAccount, src => src.LocalAccount)
            .Map(dest => dest.InternationalBankAccount, src =>
                src.InternationalAccount);

        config.NewConfig<LocalBankAccount, GetLocalBankAccountResponse>();
        config.NewConfig<InternationalBankAccount, GetInternationalBankAccountResponse>();

        config.NewConfig<FinancialAccount, GetFinancialAccountTitleResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Title, src =>
                src.PriceUnit != null
                    ? $"{src.AccountType.GetDisplayName()} - " +
                      $"{(src.AccountType == FinancialAccountType.Cash
                          ? src.PriceUnit.Title
                          : src.AccountType == FinancialAccountType.LocalBankAccount && src.LocalAccount != null
                              ? src.LocalAccount.AccountNumber
                              : src.AccountType == FinancialAccountType.InternationalBankAccount && src.InternationalAccount != null
                                  ? src.InternationalAccount.AccountNumber
                                  : string.Empty)}" : string.Empty);


        #endregion

        #region Invoices

        config.NewConfig<Invoice, GetInvoiceListResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.CustomerFullName, src => src.Customer != null ? src.Customer.FullName : string.Empty)
            .Map(dest => dest.AmountUnit,
                src => MapContext.Current.GetService<IPriceUnitRepository>()
                    .Get(new PriceUnitsSetAsDefaultSpecification())
                    .FirstOrDefault()!.Title)
            .Map(dest => dest.TotalAmount, src => src.TotalAmount * (src.ExchangeRate ?? 1))
            .Map(dest => dest.PaymentStatus,
                src => Math.Abs(src.TotalUnpaidAmount - 0m) < 0.01m
                    ? InvoicePaymentStatus.Paid
                    : src.DueDate.HasValue && src.DueDate.Value < DateOnly.FromDateTime(DateTime.Today)
                        ? InvoicePaymentStatus.Overdue
                        : InvoicePaymentStatus.HasDebt);

        config.NewConfig<Invoice, GetInvoiceResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Customer, src => src.Customer)
            .Map(dest => dest.InvoiceProductItems, src => src.ProductItems)
            .Map(dest => dest.InvoiceCoinItems, src => src.CoinItems)
            .Map(dest => dest.InvoiceCurrencyItems, src => src.CurrencyItems)
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

        config.NewConfig<InvoiceProductItem, GetInvoiceProductItemResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        config.NewConfig<InvoiceCoinItem, GetInvoiceCoinItemResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        config.NewConfig<InvoiceCurrencyItem, GetInvoiceCurrencyItemResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        config.NewConfig<InvoiceDiscount, GetInvoiceDiscountResponse>();

        config.NewConfig<InvoiceExtraCost, GetInvoiceExtraCostsResponse>();

        #endregion

        #region InvoicePayments

        config.NewConfig<InvoicePayment, GetInvoicePaymentResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.PriceUnit, src => src.PriceUnit)
            .Map(dest => dest.VoucherId, src =>
                src.PaymentVoucherId != null ? src.PaymentVoucherId.Value.Value : (Guid?)null)
            .Map(dest => dest.FinancialAccount, src => src.SourceFinancialAccount);

        #endregion

        #region LedgerAccounts

        config.NewConfig<LedgerAccount, GetLedgerAccountResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.ParentAccount, src => src.ParentAccount)
            .Map(dest => dest.IsSystemAccount, src => src.IsSystemAccount)
            .Map(dest => dest.AccountType, src => src.AccountType);

        #endregion

        #region Reporting

        config.NewConfig<Invoice, GetInvoiceDetailResponse>()
            .Map(dest => dest.Customer, src => src.Customer)
            .Map(dest => dest.InvoiceItems, src => src.ProductItems)

            // Check that the first item itself is not null before using it.
            .Map(dest => dest.TaxPercent,
                src => (src.ProductItems.FirstOrDefault() != null)
                    ? $"{src.ProductItems.First().TaxPercent.ToCurrencyFormat(null)}%"
                    : null)

            .Map(dest => dest.ProfitPercent,
                src => (src.ProductItems.FirstOrDefault() != null)
                    ? $"{src.ProductItems.First().ProfitPercent.ToCurrencyFormat(null)}%"
                    : null)

            .Map(dest => dest.DailyGramPrice,
                src => (src.ProductItems.FirstOrDefault() != null)
                    ? $"{src.ProductItems.First().GramPrice.ToCurrencyReportFormat(src.PriceUnit == null ? null : src.PriceUnit.Title)}"
                    : null)

            .Map(dest => dest.GoldUnitType,
                src => (src.ProductItems.FirstOrDefault() != null)
                    ? src.ProductItems.First().Product == null ? GoldUnitType.Gram : src.ProductItems.First().Product!.GoldUnitType
                    : GoldUnitType.Gram)

            // The rest of the mappings were already safe against this issue.
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
            .Map(dest => dest.CaratType, src => src.CaratType)
            .Map(dest => dest.ProductCategoryTitle,
                src => src.ProductCategory != null ? src.ProductCategory.Title : string.Empty);

        #endregion

        #region PaymentVouchers

        config.NewConfig<PaymentVoucher, GetPaymentVoucherListResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.VoucherNumber, src => src.VoucherNumber)
            .Map(dest => dest.PaymentDate, src => src.PaymentDate)
            .Map(dest => dest.Amount, src => src.Amount * (src.ExchangeRate ?? 1))
            .Map(dest => dest.PriceUnit, src => MapContext.Current.GetService<IPriceUnitRepository>()
                .Get(new PriceUnitsSetAsDefaultSpecification()).FirstOrDefault()!.Title)
            .Map(dest => dest.VoucherStatus, src => VoucherStatus.Pending) // TODO: Remove this!
            .Map(dest => dest.SupplierName,
                src => src.DestinationFinancialAccount != null && src.DestinationFinancialAccount.Customer != null
                    ? src.DestinationFinancialAccount.Customer.FullName
                    : string.Empty)
            .Map(dest => dest.SupplierPhoneNumber, src => src.DestinationFinancialAccount != null && src.DestinationFinancialAccount.Customer != null
                ? src.DestinationFinancialAccount.Customer.PhoneNumber
                : string.Empty)
            .Map(dest => dest.FinancialAccountType, src => src.SourceFinancialAccount != null ? src.SourceFinancialAccount.AccountType : FinancialAccountType.Cash);

        config.NewConfig<PaymentVoucher, GetPaymentVoucherResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.PaymentDate, src => new DateTime(src.PaymentDate.Year, src.PaymentDate.Month, src.PaymentDate.Day))
            .Map(dest => dest.VoucherNumber, src => src.VoucherNumber)
            .Map(dest => dest.Amount, src => src.Amount)
            .Map(dest => dest.PriceUnit, src => src.VoucherPriceUnit)
            .Map(dest => dest.Customer,
                src => src.DestinationFinancialAccount != null && src.DestinationFinancialAccount.Customer != null
                    ? src.DestinationFinancialAccount.Customer
                    : null)
            .Map(dest => dest.SourceFinancialAccount, src => src.SourceFinancialAccount)
            .Map(dest => dest.DestinationFinancialAccount, src => src.DestinationFinancialAccount);

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
            //.Map(dest => dest.InvoiceId, src => src.SellInvoiceProductItem != null ? src.SellInvoiceProductItem.InvoiceId.Value : (Guid?)null)
            // TODO: Remove this mapping when SellProduct navigation property is removed from Product entity
            .Map(dest => dest.DateTime, src => src.CreatedAt)
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
    }
}
