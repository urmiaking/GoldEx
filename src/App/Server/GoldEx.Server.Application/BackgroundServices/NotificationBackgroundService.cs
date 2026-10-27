using GoldEx.Server.Application.Services;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.NotificationAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.Routings;
using GoldEx.Sdk.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.BackgroundServices;

public sealed class NotificationBackgroundService(IServiceScopeFactory scopes, ILogger<NotificationBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var catalogScope = scopes.CreateScope();
                var catalog = catalogScope.ServiceProvider.GetRequiredService<INotificationPushRepository>();
                var stores = await catalog.ActiveStores.AsNoTracking().ToListAsync(stoppingToken);
                foreach (var store in stores)
                {
                    try
                    {
                        using var scope = scopes.CreateScope();
                        scope.ServiceProvider.GetRequiredService<StoreContext>().SetStore(store.Id.Value, store.Slug);
                        var invoiceRepository = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
                        await using var transaction = await invoiceRepository.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, stoppingToken);
                        var invoices = await invoiceRepository.GetOverdueInvoicesAsync(stoppingToken);
                        var notifications = invoices.Select(invoice => Notification.CreateInvoiceNotification(
                            NotificationMessageBuilder.BuildTitle(),
                            NotificationMessageBuilder.BuildMessage(invoice.InvoiceNumber, invoice.Customer!.FullName,
                                invoice.TotalAmount, invoice.TotalUnpaidAmount, invoice.PriceUnit!.Title),
                            [NotificationButton.CreateViewButton(ClientRoutes.Invoices.SetInvoice.FormatRoute(new { id = invoice.Id.Value })),
                             NotificationButton.CreateSendButton(ApiUrls.Invoices.SendReminder(invoice.Id.Value))], invoice.Id, store.Id)).ToList();
                        if (notifications.Count > 0)
                            await scope.ServiceProvider.GetRequiredService<IServerNotificationService>().CreateNotificationsAsync(notifications, stoppingToken);
                        await transaction.CommitAsync(stoppingToken);
                        await scope.ServiceProvider.GetRequiredService<INotificationPushProcessor>().ProcessAsync(stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
                    catch (Exception ex) { logger.LogError(ex, "Notification processing failed for store {StoreId}; retrying next cycle.", store.Id.Value); }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Notification cycle failed; retrying next cycle."); }
            try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
