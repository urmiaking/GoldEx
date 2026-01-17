using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.SettingAggregate;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerSmsService
{
    Task SendInvoiceDueMessageAsync(Invoice invoice, Setting appSetting, CancellationToken cancellationToken = default);
}