using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.NotificationAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface INotificationRepository : IRepository<Notification>,
    ICreateRepository<Notification>,
    IUpdateRepository<Notification>,
    IDeleteRepository<Notification>;