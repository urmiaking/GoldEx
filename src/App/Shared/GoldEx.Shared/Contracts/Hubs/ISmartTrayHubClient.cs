using GoldEx.Shared.DTOs.SmartTrays;

namespace GoldEx.Shared.Contracts.Hubs;

public interface ISmartTrayHubClient
{
    Task TrayUpdated(SmartTrayDto tray);
    Task ItemAdded(Guid trayId, SmartTrayItemDto item);
    Task ItemReturned(Guid trayId, SmartTrayItemDto item);
    Task ItemSold(Guid trayId, SmartTrayItemDto item);
    Task ItemRemoved(Guid trayId, string barcode);
    Task DiscrepancyReported(Guid trayId, string notes);
    Task TrayClosed(Guid trayId);
}
