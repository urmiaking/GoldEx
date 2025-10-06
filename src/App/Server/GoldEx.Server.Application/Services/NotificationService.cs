using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.NotificationAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Notifications;
using GoldEx.Shared.DTOs.Notifications;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class NotificationService(INotificationRepository repository, IMapper mapper) : IServerNotificationService, INotificationService
{
    public Task CreateNotificationsAsync(List<Notification> notifications, CancellationToken cancellationToken = default)
    {
        return repository.CreateRangeAsync(notifications, cancellationToken);
    }

    public async Task<List<GetNotificationResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await repository
            .Get(new NotificationsByReadStatusSpecification(false))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetNotificationResponse>>(list);
    }

    public async Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await repository
            .Get(new NotificationsByIdSpecification(new NotificationId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        notification.MarkAsRead();

        await repository.UpdateAsync(notification, cancellationToken);
    }

    public async Task MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        var list = await repository
            .Get(new NotificationsByReadStatusSpecification(false))
            .ToListAsync(cancellationToken);

        list.ForEach(x => x.MarkAsRead());

        await repository.UpdateRangeAsync(list, cancellationToken);
    }
}