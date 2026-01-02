using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.DTOs.Reporting;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class ReportingMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Transaction, LedgerAccountStatementRpResponse>()
            .Map(dest => dest.InvoiceId, 
                src => src.InvoiceId != null ? src.InvoiceId.Value.Value : (Guid?)null)
            .Map(dest => dest.InvoicePaymentId,
                src => src.InvoicePaymentId != null ? src.InvoicePaymentId.Value.Value : (Guid?)null)
            .Map(dest => dest.InventoryExitId,
                src => src.InventoryExitId != null ? src.InventoryExitId.Value.Value : (Guid?)null)
            .Map(dest => dest.InventoryEntryId,
                src => src.InventoryEntryId != null ? src.InventoryEntryId.Value.Value : (Guid?)null)
            .Map(dest => dest.InventoryStockId,
                src => src.InventoryStockId != null ? src.InventoryStockId.Value.Value : (Guid?)null)
            .Map(dest => dest.MeltingBatchId,
                src => src.MeltingBatchId != null ? src.MeltingBatchId.Value.Value : (Guid?)null)
            .Map(dest => dest.PaymentVoucherId,
                src => src.PaymentVoucherId != null ? src.PaymentVoucherId.Value.Value : (Guid?)null)
            .Map(dest => dest.PriceUnitTitle,
                src => src.PriceUnit != null ? src.PriceUnit.Title : string.Empty);
    }
}