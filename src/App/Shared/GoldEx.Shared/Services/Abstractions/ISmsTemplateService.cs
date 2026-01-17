using GoldEx.Shared.DTOs.SmsTemplates;

namespace GoldEx.Shared.Services.Abstractions;

public interface ISmsTemplateService
{
    Task<List<SmsTemplateResponse>> GetListAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(List<SmsTemplateRequest> requests, CancellationToken cancellationToken = default);
}