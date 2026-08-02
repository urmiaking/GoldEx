using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.CustomerTransfers;
using GoldEx.Shared.DTOs.PaymentVouchers;

namespace GoldEx.Shared.Services.Abstractions;

public interface ICustomerTransferVoucherService
{
    Task<PagedList<GetCustomerTransferVoucherListResponse>> GetListAsync(
        RequestFilter filter,
        CustomerTransferVoucherFilter voucherFilter,
        CancellationToken cancellationToken = default);

    Task<GetCustomerTransferVoucherResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetCustomerTransferVoucherResponse> GetAsync(long voucherNumber, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateCustomerTransferVoucherRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateCustomerTransferVoucherRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetVoucherNumberResponse> GetLastNumberAsync(CancellationToken cancellationToken = default);
}
