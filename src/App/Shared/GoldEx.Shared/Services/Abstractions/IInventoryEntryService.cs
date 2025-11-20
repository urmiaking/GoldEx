using GoldEx.Shared.DTOs.InventoryEntries;

namespace GoldEx.Shared.Services.Abstractions;

public interface IInventoryEntryService
{
    Task CreateAsync(CreateInventoryEntryRequest request, CancellationToken cancellationToken = default);
    Task<ProcessExcelResponse> ProcessExcelAsync(ProcessExcelRequest request, CancellationToken cancellationToken = default);
}