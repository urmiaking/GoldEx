using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

public class InvoicesMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
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
            .Map(dest => dest.InvoiceUsedProducts, src => src.UsedProducts)
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
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.CostPriceUnitId,
                src => src.CostPriceUnitId.HasValue ? src.CostPriceUnitId.Value.Value : (Guid?)null)
            .Map(dest => dest.CostPriceUnitTitle,
                src => src.CostPriceUnit != null ? src.CostPriceUnit.Title : string.Empty);

        config.NewConfig<InvoiceCoinItem, GetInvoiceCoinItemResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        config.NewConfig<InvoiceCurrencyItem, GetInvoiceCurrencyItemResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        config.NewConfig<InvoiceDiscount, GetInvoiceDiscountResponse>();

        config.NewConfig<InvoiceExtraCost, GetInvoiceExtraCostsResponse>();

        config.NewConfig<InvoiceUsedProduct, GetInvoiceUsedProductResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        config.NewConfig<InvoicePayment, GetInvoicePaymentResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.PriceUnit, src => src.PriceUnit)
            .Map(dest => dest.VoucherId, src =>
                src.PaymentVoucherId != null ? src.PaymentVoucherId.Value.Value : (Guid?)null)
            .Map(dest => dest.FinancialAccount, src => src.SourceFinancialAccount)
            .Map(dest => dest.FinancialAccounts,
                src => MapContext.Current.GetService<IFinancialAccountService>()
                    .GetTitlesAsync(null, src.PriceUnitId.Value, CancellationToken.None).GetAwaiter().GetResult());
    }
}