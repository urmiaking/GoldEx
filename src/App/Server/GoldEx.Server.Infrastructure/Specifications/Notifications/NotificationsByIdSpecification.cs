using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.NotificationAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Notifications;

public class NotificationsByIdSpecification(NotificationId id)
    : SpecificationBase<Notification>(n => n.Id == id);