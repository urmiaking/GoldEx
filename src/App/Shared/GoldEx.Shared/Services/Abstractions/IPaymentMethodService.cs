using GoldEx.Shared.DTOs.PaymentMethods;

namespace GoldEx.Shared.Services.Abstractions;

public interface IPaymentMethodService
{
    Task<List<GetPaymentMethodResponse>> GetListAsync(CancellationToken cancellationToken = default);
    Task<List<GetPaymentMethodResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GetPaymentMethodResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(CreatePaymentMethodRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdatePaymentMethodRequest request, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid id, UpdatePaymentMethodStatusRequest request, CancellationToken cancellationToken = default);
}