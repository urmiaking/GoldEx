using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.CheckPayments;

namespace GoldEx.Shared.Services.Abstractions;

public interface ICheckPaymentService
{
    Task<PagedList<GetCheckPaymentListResponse>> GetListAsync(
        RequestFilter filter,
        CheckPaymentFilter checkPaymentFilter,
        CancellationToken cancellationToken = default);

    Task AcceptAsync(Guid checkPaymentId, AcceptCheckPaymentRequest request, CancellationToken cancellationToken = default);
    Task ReturnAsync(Guid checkPaymentId, ReturnCheckPaymentRequest request, CancellationToken cancellationToken = default);
}
