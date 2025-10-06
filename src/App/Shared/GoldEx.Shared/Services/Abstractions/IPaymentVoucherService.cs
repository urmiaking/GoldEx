using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.PaymentVouchers;

namespace GoldEx.Shared.Services.Abstractions;

public interface IPaymentVoucherService
{
    Task<PagedList<GetPaymentVoucherListResponse>> GetListAsync(RequestFilter filter, PaymentVoucherFilter voucherFilter,
        Guid? customerId, CancellationToken cancellationToken = default);
    Task<List<GetPaymentVoucherResponse>> GetPendingListAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<GetPaymentVoucherResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetPaymentVoucherResponse> GetAsync(long voucherNumber, CancellationToken cancellationToken = default);
    Task CreateAsync(PaymentVoucherRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, PaymentVoucherRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetVoucherNumberResponse> GetLastNumberAsync(CancellationToken cancellationToken = default);
}