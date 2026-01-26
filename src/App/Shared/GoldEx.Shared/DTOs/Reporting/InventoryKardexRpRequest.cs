namespace GoldEx.Shared.DTOs.Reporting;

public record InventoryKardexRpRequest(
    Guid? ProductId,
    Guid? CoinInstanceId,
    Guid? CurrencyId,
    DateTime? FromDate = null,
    DateTime? ToDate = null);