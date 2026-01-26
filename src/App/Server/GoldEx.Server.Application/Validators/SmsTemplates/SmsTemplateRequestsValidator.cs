using System.Text.RegularExpressions;
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

    private static readonly Regex PlaceholderRegex =
        new(@"\(([^:)]+)(?::([^)]+))?\)", RegexOptions.Compiled);

    public SmsTemplateRequestValidator(ISmsTemplateRepository repository)
    {
        _repository = repository;
        RuleFor(x => x.Body)
            .MustAsync(MatchParameters)
            .WithMessage("پارامترهای قالب پیامک با پارامترهای تعریف شده مطابقت ندارد");
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
        if (string.IsNullOrWhiteSpace(body))
            return true;

        if (string.IsNullOrWhiteSpace(parameters))
            return !PlaceholderRegex.IsMatch(body);

        var usedPlaceholders = PlaceholderRegex.Matches(body)
            .Select(m => Normalize(m.Groups[1].Value))
            .Distinct()
            .ToList();

        var allowedParameters = parameters
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(Normalize)
            .ToHashSet();

        return usedPlaceholders.All(p => allowedParameters.Contains(p));
    }

    private static string Normalize(string value)
    {
        return value
            .Replace(" ", "")
            .Replace("_", "")
            .Trim();
    }
}