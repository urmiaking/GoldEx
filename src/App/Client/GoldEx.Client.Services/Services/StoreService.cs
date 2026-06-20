using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Stores;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class StoreService(HttpClient client, JsonSerializerOptions jsonOptions) : IStoreService
{
    public async Task<List<UserStoreDto>> GetUserStoresAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Stores.GetUserStores(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<UserStoreDto>>(jsonOptions, cancellationToken);

        return result ?? [];
    }

    public async Task SwitchStoreAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(ApiUrls.Stores.Switch(storeId), null, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task<PagedList<GetStoreRequest>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Stores.GetList(filter), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetStoreRequest>>(jsonOptions, cancellationToken);

        return result ?? new PagedList<GetStoreRequest> { Data = [] };
    }

    public async Task CreateStoreAsync(StoreRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.Stores.Create(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task UpdateStoreAsync(Guid id, StoreRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.Stores.Update(id), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task DeleteStoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(ApiUrls.Stores.Delete(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task<List<Guid>> GetStoreUsersAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Stores.GetStoreUsers(storeId), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<Guid>>(jsonOptions, cancellationToken);

        return result ?? [];
    }

    public async Task AssignStoreUsersAsync(Guid storeId, AssignStoreUsersRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.Stores.AssignStoreUsers(storeId), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}
