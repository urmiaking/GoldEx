using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.MeltingBatches;

namespace GoldEx.Shared.Services.Abstractions;

public interface IMeltingBatchService
{
    Task<PagedList<GetMeltingBatchResponse>> GetListAsync(RequestFilter requestFilter,
        MeltingBatchFilter filter,
        CancellationToken cancellationToken = default);
    Task<GetMeltingBatchResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CreateMeltingBatchResponse> CreateAsync(MeltingBatchRequestDto request, CancellationToken cancellationToken = default);
    Task SendToLabAsync(Guid id, SendToLabRequestDto request, CancellationToken cancellationToken = default);
    Task CompleteMeltingAsync(Guid id, CompleteMeltingRequestDto request, CancellationToken cancellationToken = default);
}