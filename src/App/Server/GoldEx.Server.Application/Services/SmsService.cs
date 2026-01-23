using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.SettingAggregate;
using GoldEx.Server.Domain.SmsLogAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.SmsTemplates;
using GoldEx.Shared.Constants;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class SmsService(ISmsSender smsSender, ISmsTemplateRepository templateRepository, ISmsLogRepository logRepository) : IServerSmsService
{
    public async Task SendInvoiceDueMessageAsync(Invoice invoice, Setting appSetting, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(invoice.Customer?.PhoneNumber))
        {
            throw new ValidationException(new List<ValidationFailure>
            {
                new(nameof(invoice), "امکان ارسال پیامک برای مشتری به دلیل عدم وجود شماره تماس وجود ندارد")
            });
        }

        var phoneNumber = invoice.Customer.PhoneNumber;

        var smsTemplate = await templateRepository
            .Get(new SmsTemplatesBySubjectSpecification(SmsTemplateSubject.DueInvoice))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Due invoice sms template not found");

        var parameterMapping = smsTemplate.Parameters.Split(',')
            .ToDictionary(parameter => parameter,
                parameter => GetParameterValue(parameter, invoice, appSetting));

        var message = GenerateMessage(smsTemplate.Body, parameterMapping);

        var delivered = await smsSender.SendAsync(phoneNumber, message, cancellationToken);

        await logRepository.CreateAsync(SmsLog.Create(message, phoneNumber, delivered), cancellationToken);
    }

    private string? GetParameterValue(string parameter, Invoice invoice, Setting appSetting)
    {
        return parameter switch {
            SmsTemplateParams.CustomerName => invoice.Customer?.FullName,
            SmsTemplateParams.CustomerPhoneNumber => invoice.Customer?.PhoneNumber,
            SmsTemplateParams.InvoiceNumber => invoice.InvoiceNumber.ToString(),
            SmsTemplateParams.InvoiceRemaining => invoice.TotalUnpaidAmount.ToCurrencyFormat(invoice.PriceUnit?.Title),
            SmsTemplateParams.InvoiceTotalAmount => invoice.TotalAmount.ToCurrencyFormat(invoice.PriceUnit?.Title),
            SmsTemplateParams.InvoiceDate => invoice.InvoiceDate.ToString("yyyy-MM-dd dddd"),
            SmsTemplateParams.InvoiceDueDate => invoice.DueDate?.ToString("yyyy-MM-dd dddd"),
            SmsTemplateParams.TodayDate => DateTime.Today.ToString("yyyy-MM-dd dddd"),
            SmsTemplateParams.InstitutionName => appSetting.InstitutionName,
            SmsTemplateParams.InstitutionPhoneNumber => appSetting.PhoneNumber,
            SmsTemplateParams.InstitutionAddress => appSetting.Address,
            _ => throw new ArgumentOutOfRangeException(nameof(parameter), parameter, null)
        };
    }

    private string GenerateMessage(string template, IReadOnlyDictionary<string, string?> parameterMapping)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentNullException(nameof(template));

        if (parameterMapping is null)
            throw new ArgumentNullException(nameof(parameterMapping));

        var values = parameterMapping
            .Select(p => new PlaceholderValue(
                Name: p.Key,
                Value: p.Value ?? string.Empty))
            .ToList();

        return SmsTemplatePlaceholderReplacer.Replace(template, values);
    }
}