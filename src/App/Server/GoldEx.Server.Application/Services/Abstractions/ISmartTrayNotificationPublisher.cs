using GoldEx.Shared.DTOs.SmartTrays;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface ISmartTrayNotificationPublisher
{
    Task TrayUpdatedAsync(SmartTrayDto tray, CancellationToken cancellationToken = default);
    Task ItemAddedAsync(Guid trayId, SmartTrayItemDto item, CancellationToken cancellationToken = default);
    Task ItemReturnedAsync(Guid trayId, SmartTrayItemDto item, CancellationToken cancellationToken = default);
    Task ItemSoldAsync(Guid trayId, SmartTrayItemDto item, CancellationToken cancellationToken = default);
    Task ItemRemovedAsync(Guid trayId, string barcode, CancellationToken cancellationToken = default);
    Task DiscrepancyReportedAsync(Guid trayId, string notes, CancellationToken cancellationToken = default);
    Task TrayClosedAsync(Guid trayId, CancellationToken cancellationToken = default);
}
