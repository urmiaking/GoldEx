using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.DTOs.SmartTrays;

namespace GoldEx.Server.Application.Services;

public class NullSmartTrayNotificationPublisher : ISmartTrayNotificationPublisher
{
    public Task TrayUpdatedAsync(SmartTrayDto tray, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task ItemAddedAsync(Guid trayId, SmartTrayItemDto item, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task ItemReturnedAsync(Guid trayId, SmartTrayItemDto item, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task ItemSoldAsync(Guid trayId, SmartTrayItemDto item, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task ItemRemovedAsync(Guid trayId, string barcode, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DiscrepancyReportedAsync(Guid trayId, string notes, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task TrayClosedAsync(Guid trayId, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
