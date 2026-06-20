using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Stores;

namespace GoldEx.Shared.Services.Abstractions;

public interface IStoreService
{
    Task<List<UserStoreDto>> GetUserStoresAsync(CancellationToken cancellationToken = default);
    Task SwitchStoreAsync(Guid storeId, CancellationToken cancellationToken = default);

    Task<PagedList<GetStoreRequest>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task CreateStoreAsync(StoreRequest request, CancellationToken cancellationToken = default);
    Task UpdateStoreAsync(Guid id, StoreRequest request, CancellationToken cancellationToken = default);
    Task DeleteStoreAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Guid>> GetStoreUsersAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task AssignStoreUsersAsync(Guid storeId, AssignStoreUsersRequest request, CancellationToken cancellationToken = default);
}
