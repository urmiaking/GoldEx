using GoldEx.Server.Domain.NotificationAggregate;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerNotificationService
{
    Task CreateNotificationsAsync(List<Notification> notifications, CancellationToken cancellationToken = default);
}