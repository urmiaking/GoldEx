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
                src => src.TotalUnpaidAmount == 0
                    ? InvoicePaymentStatus.Paid
                    : src.TotalUnpaidAmount == src.TotalAmount
                        ? InvoicePaymentStatus.Unpaid
                        : InvoicePaymentStatus.PartiallyPaid);

        config.NewConfig<Invoice, GetInvoiceResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Customer, src => src.Customer)
            .Map(dest => dest.InvoiceItems, src => src.Items)
            .Map(dest => dest.InvoiceDiscounts, src => src.Discounts)
            .Map(dest => dest.InvoiceExtraCosts, src => src.ExtraCosts)
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

        config.NewConfig<InvoicePayment, GetInvoicePaymentResponse>();

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
