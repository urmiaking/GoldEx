using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryExits;

namespace GoldEx.Shared.Services.Abstractions;

public interface IInventoryExitService
{
    Task<PagedList<InventoryExitResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task ExitAsync(CreateInventoryExitRequest request, CancellationToken cancellationToken = default);
}