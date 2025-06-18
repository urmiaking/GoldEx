using GoldEx.Shared.DTOs.Invoices;

namespace GoldEx.Shared.Services;

public interface IInvoiceService
{
    Task CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken);
}