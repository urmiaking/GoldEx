using GoldEx.Shared.DTOs.Notifications;

namespace GoldEx.Shared.Services.Abstractions;

public interface INotificationService
{
    Task<List<GetNotificationResponse>> GetListAsync(bool? isRead, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}