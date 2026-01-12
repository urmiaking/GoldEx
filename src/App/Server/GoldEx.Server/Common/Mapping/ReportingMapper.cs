using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Infrastructure.Models;
using GoldEx.Shared.DTOs.Reporting;
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
            .Map(dest => dest.RemainingPrice, src => src.TotalUnpaidAmount)
            .Map(dest => dest.TotalPrice, src => src.TotalAmountWithDiscountsAndExtraCosts)
            .Map(dest => dest.TotalProfit, src => src.TotalProfitAmount)
            .Map(dest => dest.TotalTax, src => src.TotalTaxAmount)
            .Map(dest => dest.TotalWage, src => src.TotalWageAmount);

        config.NewConfig<Invoice, PurchaseInvoiceRpResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.PriceUnit, src => src.PriceUnit != null ? src.PriceUnit.Title : string.Empty)
            .Map(dest => dest.RemainingPrice, src => src.TotalUnpaidAmount)
            .Map(dest => dest.TotalPrice, src => src.TotalAmountWithDiscountsAndExtraCosts);

        config.NewConfig<InvoicePayment, PaymentRpResponse>()
            .Map(dest => dest.Customer, src => src.Invoice!.Customer)
            .Map(dest => dest.InvoiceId, src => src.InvoiceId.Value)
            .Map(dest => dest.InvoiceNumber, src => src.Invoice!.InvoiceNumber)
            .Map(dest => dest.InvoiceType, src => src.Invoice!.InvoiceType)
            .Map(dest => dest.PriceUnit, src => src.PriceUnit!.Title)
            .Map(dest => dest.Description, src => PaymentDescriptionBuilder.Build(src, true));
    }
}