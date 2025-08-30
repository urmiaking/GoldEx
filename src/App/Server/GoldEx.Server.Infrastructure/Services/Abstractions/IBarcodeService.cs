using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Services.Abstractions;

public interface IBarcodeService
{
    Task<string> GenerateProductBarcodeAsync(Product product, CancellationToken cancellationToken = default);
    Task ValidateBarcodeUniquenessAsync(string barcode, CancellationToken cancellationToken = default);
}