using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryEntries;

namespace GoldEx.Shared.Services.Abstractions;

public interface IInventoryEntryService
{
    Task<PagedList<InventoryEntryResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateInventoryEntryRequest request, CancellationToken cancellationToken = default);
    Task<ProcessExcelResponse> ProcessExcelAsync(ProcessExcelRequest request, CancellationToken cancellationToken = default);
    Task RollbackAsync(Guid id, CancellationToken cancellationToken = default);
}