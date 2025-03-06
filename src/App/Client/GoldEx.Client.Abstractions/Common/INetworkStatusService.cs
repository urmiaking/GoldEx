namespace GoldEx.Client.Abstractions.Common;

public interface INetworkStatusService
{
    Task<bool> IsOnlineAsync(CancellationToken cancellationToken = default);
}