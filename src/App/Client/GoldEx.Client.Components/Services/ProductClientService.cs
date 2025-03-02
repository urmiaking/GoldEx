using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Client.Offline.Domain.ProductAggregate;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using MapsterMapper;

namespace GoldEx.Client.Components.Services;

public class ProductClientService(
    HttpClient client,
    JsonSerializerOptions jsonOptions,
    IMapper mapper,
    IProductService<Product> service) : IProductClientService
{
    public async Task<PagedList<GetProductResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var products = await service.GetListAsync(filter, cancellationToken);

        return mapper.Map<PagedList<GetProductResponse>>(products);
        //using var response = await client.GetAsync(ApiUrls.Products.GetList(filter), cancellationToken);

        //if (!response.IsSuccessStatusCode)
        //    throw HttpRequestFailedException.GetException(response.StatusCode, response);

        //var result = await response.Content.ReadFromJsonAsync<PagedList<GetProductResponse>>(jsonOptions, cancellationToken);

        //return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetProductResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Products.Get(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetProductResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetProductResponse?> GetAsync(string barcode, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Products.GetByBarcode(barcode), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetProductResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = new Product(request.Name, request.Barcode, request.Weight, request.Wage,
            request.ProductType, request.WageType, request.CaratType);

        await service.CreateAsync(product, cancellationToken);

        //using var response = await client.PostAsJsonAsync(ApiUrls.Products.Create(), request, jsonOptions, cancellationToken);

        //if (!response.IsSuccessStatusCode)
        //    throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.Products.Update(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.Products.Delete(id), jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}