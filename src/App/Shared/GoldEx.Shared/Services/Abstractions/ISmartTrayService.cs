using GoldEx.Shared.DTOs.SmartTrays;

namespace GoldEx.Shared.Services.Abstractions;

public interface ISmartTrayService
{
    Task<List<SmartTrayDto>> GetActiveTraysAsync(CancellationToken cancellationToken = default);
    Task<SmartTrayDto> GetTrayByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SmartTrayDto> CreateTrayAsync(CreateSmartTrayRequest request, CancellationToken cancellationToken = default);
    Task<SmartTrayItemDto> AddItemByBarcodeAsync(Guid trayId, string barcode, CancellationToken cancellationToken = default);
    Task<SmartTrayItemDto> ReturnItemByBarcodeAsync(Guid trayId, string barcode, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(Guid trayId, string barcode, CancellationToken cancellationToken = default);
    Task<SmartTrayItemDto> MarkItemSoldAsync(Guid trayId, string barcode, Guid invoiceId, CancellationToken cancellationToken = default);
    Task CloseTrayAsync(Guid trayId, CancellationToken cancellationToken = default);
    Task ReportDiscrepancyAsync(Guid trayId, ReportSmartTrayDiscrepancyRequest request, CancellationToken cancellationToken = default);
    Task CancelTrayAsync(Guid trayId, CancellationToken cancellationToken = default);
}
