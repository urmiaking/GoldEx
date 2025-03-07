namespace GoldEx.Client.Abstractions.SyncServices;

public interface ISyncService
{
    Task SynchronizeAsync(CancellationToken cancellationToken);
}