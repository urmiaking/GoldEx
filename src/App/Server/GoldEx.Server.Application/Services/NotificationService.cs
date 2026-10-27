using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Application.Abstractions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.NotificationAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.Notifications;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class NotificationService(INotificationRepository repository, INotificationPushRepository pushRepository,
    IUserContext user, IMapper mapper) : IServerNotificationService, INotificationService
{
    private Guid UserId => user.GetUserId() ?? throw new UnauthorizedAccessException();
    public Task CreateNotificationsAsync(List<Notification> notifications, CancellationToken cancellationToken = default) =>
        repository.CreateRangeAsync(notifications, cancellationToken);

    public async Task<List<GetNotificationResponse>> GetListAsync(bool? isRead, CancellationToken cancellationToken = default)
    {
        var states = pushRepository.UserStates.Where(x => x.UserId == UserId);
        var query = pushRepository.Notifications.AsNoTracking()
            .Where(x => !states.Any(s => s.NotificationId == x.Id && s.Dismissed));
        if (isRead.HasValue)
            query = query.Where(x => (x.IsRead || states.Any(s => s.NotificationId == x.Id && s.IsRead)) == isRead.Value);
        var list = await query.OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(cancellationToken);
        var readIds = await states.Where(x => x.IsRead).Select(x => x.NotificationId).ToListAsync(cancellationToken);
        return list.Select(x => mapper.Map<GetNotificationResponse>(x) with { IsRead = x.IsRead || readIds.Contains(x.Id) }).ToList();
    }

    public Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default) => SetStateAsync(id, false, cancellationToken);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => SetStateAsync(id, true, cancellationToken);
    private async Task SetStateAsync(Guid id, bool dismiss, CancellationToken ct)
    {
        var notification = await pushRepository.Notifications.FirstOrDefaultAsync(x => x.Id == new NotificationId(id), ct)
            ?? throw new NotFoundException();
        var state = await pushRepository.UserStates.FirstOrDefaultAsync(x => x.NotificationId == notification.Id && x.UserId == UserId, ct);
        if (state is null) { state = NotificationUserState.Create(notification, UserId); pushRepository.Add(state); }
        if (dismiss) state.Dismiss(); else state.MarkRead();
        await pushRepository.SaveAsync(ct);
    }
    public async Task MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        var states = await pushRepository.UserStates.Where(x => x.UserId == UserId).ToListAsync(cancellationToken);
        var notifications = await pushRepository.Notifications.Where(x => !x.IsRead).ToListAsync(cancellationToken);
        foreach (var notification in notifications)
        {
            var state = states.FirstOrDefault(x => x.NotificationId == notification.Id);
            if (state is null) { state = NotificationUserState.Create(notification, UserId); pushRepository.Add(state); }
            state.MarkRead();
        }
        await pushRepository.SaveAsync(cancellationToken);
    }
}
