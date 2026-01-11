using GoldEx.Server.Domain.InvoiceAggregate;
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
    }
}