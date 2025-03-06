namespace GoldEx.Client.Abstractions.SyncServices;

public interface IProductSyncService
{
    Task SynchronizeAsync(CancellationToken cancellationToken);
}