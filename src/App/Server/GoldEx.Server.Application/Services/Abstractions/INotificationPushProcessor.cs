namespace GoldEx.Server.Application.Services.Abstractions;

public interface INotificationPushProcessor
{
    Task ProcessAsync(CancellationToken ct);
}
