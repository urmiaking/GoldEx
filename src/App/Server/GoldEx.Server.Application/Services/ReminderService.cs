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
internal class ReminderService(IServerSmsService smsService, IInvoiceRepository invoiceRepository, ISettingRepository settingRepository) : IServerReminderService
{
    public async Task SendReminderAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await invoiceRepository
            .Get(new InvoicesByIdSpecification(new InvoiceId(invoiceId)))
            .AsNoTracking()
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        var appSetting = await settingRepository
            .Get(new SettingsDefaultSpecification())
            .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Settings not found");

        await smsService.SendInvoiceDueMessageAsync(invoice, appSetting, cancellationToken);
    }
}