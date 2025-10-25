using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Services.Abstractions;

internal interface IServerProductService
{
    Task<Product> CreateProductAsync(ProductRequestDto request, CancellationToken cancellationToken = default);
    Task<Product> UpdateAsync(ProductId id, ProductRequestDto request, InvoiceType invoiceType, CancellationToken cancellationToken = default);
    Task SyncUsedProductsForInvoiceAsync(Invoice invoice, IEnumerable<InvoiceUsedProductDto> usedProductDtos, CancellationToken cancellationToken = default);
    Task SyncProductItemsAsync(Invoice invoice, IEnumerable<InvoiceProductItemDto> requestedItems, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(List<Product> productList, CancellationToken cancellationToken = default);
}
