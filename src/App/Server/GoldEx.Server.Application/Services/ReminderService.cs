using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.Settings;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ReminderService(ISmsSender smsSender, IInvoiceRepository invoiceRepository, ISettingRepository settingRepository) : IServerReminderService
{
    public async Task SendReminderAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository
            .Get(new InvoicesByIdSpecification(new InvoiceId(invoiceId)))
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.PriceUnit)
            .Include(x => x.UnpaidPriceUnit)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        if (string.IsNullOrEmpty(invoice.Customer?.PhoneNumber))
        {
            throw new ValidationException(new List<ValidationFailure>
            {
                new("invoiceId", "امکان ارسال پیامک برای مشتری به دلیل عدم وجود شماره تماس وجود ندارد")
            });
        }

        var appSetting = await settingRepository
            .Get(new SettingsDefaultSpecification())
            .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Settings not found");

        // TODO: load the message template from the database
        var message = $"""
            {invoice.Customer.FullName} عزیز
            فاکتور شماره {invoice.InvoiceNumber} شما به مبلغ {invoice.TotalAmount:N0} {invoice.PriceUnit?.Title} صادر شده است.
            مبلغ پرداخت نشده این فاکتور {invoice.TotalUnpaidAmount:N0} {invoice.PriceUnit?.Title} می‌باشد.
            لطفا در اسرع وقت نسبت به تسویه آن اقدام فرمایید.
            با تشکر، {appSetting.InstitutionName}
            """;

        // TODO: log the message to the database
        await smsSender.SendAsync(invoice.Customer.PhoneNumber, message, cancellationToken);
    }
}