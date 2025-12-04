namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerInvoiceService
{
    Task<byte[]> GeneratePdfAsync(Guid id, CancellationToken cancellationToken = default);
}