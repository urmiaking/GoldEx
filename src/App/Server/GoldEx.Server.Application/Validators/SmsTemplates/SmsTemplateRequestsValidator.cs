using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Domain.SmsTemplateAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.SmsTemplates;
using GoldEx.Shared.DTOs.SmsTemplates;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.SmsTemplates;

[ScopedService]
internal class SmsTemplateRequestsValidator : AbstractValidator<List<SmsTemplateRequest>>
{
    public SmsTemplateRequestsValidator(ISmsTemplateRepository repository)
    {
        RuleForEach(x => x)
            .SetValidator(new SmsTemplateRequestValidator(repository))
            .WithMessage("پارامترهای قالب پیامک با پارامترهای تعریف شده مطابقت ندارد");
    }
}

internal class SmsTemplateRequestValidator : AbstractValidator<SmsTemplateRequest>
{
    private readonly ISmsTemplateRepository _repository;
    public SmsTemplateRequestValidator(ISmsTemplateRepository repository)
    {
        _repository = repository;
        RuleFor(x => x.Body)
            .MustAsync(MatchParameters);
    }

    private async Task<bool> MatchParameters(SmsTemplateRequest request, string body, CancellationToken cancellationToken = default)
    {
        var smsTemplate = await _repository
            .Get(new SmsTemplatesByIdSpecification(new SmsTemplateId(request.Id)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return IsMatch(body, smsTemplate.Parameters);
    }

    private bool IsMatch(string body, string parameters)
    {
        // TODO: implement this
        return true;
    }
}