using GoldEx.Server.Domain.NotificationAggregate;
using GoldEx.Shared.DTOs.Notifications;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class NotificationMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Notification, GetNotificationResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.InvoiceId, src => src.InvoiceId.HasValue ? src.InvoiceId.Value.Value : (Guid?)null)
            .Map(dest => dest.Buttons, src => src.Buttons);

    }
}