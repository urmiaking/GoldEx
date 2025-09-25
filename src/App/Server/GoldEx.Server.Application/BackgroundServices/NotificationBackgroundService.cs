using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.NotificationAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.Routings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.BackgroundServices;

public class NotificationBackgroundService(
    IServiceScopeFactory serviceScopeFactory, 
    ILogger<NotificationBackgroundService> logger) : BackgroundService
{
    private const string ClassName = nameof(NotificationBackgroundService);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Name} is running.", ClassName);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = serviceScopeFactory.CreateScope();

                var invoiceRepository = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();

                var invoices = await invoiceRepository.GetOverdueInvoicesAsync(stoppingToken);

                if (invoices.Any())
                {
                    var notificationService = scope.ServiceProvider.GetRequiredService<IServerNotificationService>();

                    List<Notification> notifications = [];

                    notifications.AddRange(invoices.Select(invoice => Notification.CreateInvoiceNotification(
                        NotificationMessageBuilder.BuildTitle(),
                        NotificationMessageBuilder.BuildMessage(invoice.InvoiceNumber,
                            invoice.Customer!.FullName,
                            invoice.TotalAmount,
                            invoice.TotalUnpaidAmount,
                            invoice.PriceUnit!.Title),
                        [
                                    NotificationButton.CreateViewButton(ClientRoutes.Invoices.SetInvoice.FormatRoute(new
                                    {
                                        id = invoice.Id.Value
                                    })),
                                    NotificationButton.CreateSendButton(ApiUrls.Invoices.SendReminder(invoice.Id.Value))
                                ],
                        invoice.Id)));

                    await notificationService.CreateNotificationsAsync(notifications, stoppingToken);
                }

                // TODO: add updateInterval to settings
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, $"{ClassName} got an exception");
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{Name} is stopping.", ClassName);
        return base.StopAsync(cancellationToken);
    }
}