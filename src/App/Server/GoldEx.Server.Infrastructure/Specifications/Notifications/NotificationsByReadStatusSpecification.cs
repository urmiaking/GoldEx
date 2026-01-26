using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.NotificationAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Notifications;

public class NotificationsByReadStatusSpecification : SpecificationBase<Notification>
{
    public NotificationsByReadStatusSpecification(bool? isRead)
    {
        if (isRead.HasValue) 
            AddCriteria(x => x.IsRead == isRead);

        ApplyOrderByDescending(x => x.CreatedAt);
    }
}