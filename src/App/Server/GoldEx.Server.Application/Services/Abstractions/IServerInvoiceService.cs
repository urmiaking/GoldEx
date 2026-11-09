using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Shared.DTOs.Invoices;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerInvoiceService
{
    Task<Invoice> PostAsync(InvoiceRequestDto request, CancellationToken cancellationToken, bool useExistingProductsAsIs = false);
    Task<byte[]> GeneratePdfAsync(Guid id, CancellationToken cancellationToken = default);
}
