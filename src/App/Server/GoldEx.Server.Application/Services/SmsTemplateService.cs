using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.SmsTemplates;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.SmsTemplates;
using GoldEx.Shared.DTOs.SmsTemplates;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class SmsTemplateService(
    ISmsTemplateRepository repository,
    IMapper mapper,
    SmsTemplateRequestsValidator validator) : ISmsTemplateService
{
    public async Task<List<SmsTemplateResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await repository.Get(new SmsTemplatesDefaultSpecification()).ToListAsync(cancellationToken);
        return mapper.Map<List<SmsTemplateResponse>>(list);
    }

    public async Task UpdateAsync(List<SmsTemplateRequest> requests, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(requests, cancellationToken);

        var list = await repository.Get(new SmsTemplatesDefaultSpecification()).ToListAsync(cancellationToken);

        foreach (var smsTemplate in list)
        {
            var request = requests.FirstOrDefault(x => x.Id == smsTemplate.Id.Value) ?? throw new NotFoundException();

            smsTemplate.SetStatus(request.IsActive);
            smsTemplate.SetBody(request.Body);
        }

        await repository.UpdateRangeAsync(list, cancellationToken);
    }
}