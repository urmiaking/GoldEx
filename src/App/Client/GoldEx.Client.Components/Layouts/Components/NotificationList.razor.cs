using GoldEx.Shared.DTOs.Notifications;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Components.Layouts.Components;

public partial class NotificationList
{
    [Parameter] public List<GetNotificationResponse> Items { get; set; } = [];
    [Parameter] public bool Busy { get; set; }
    [Parameter] public EventCallback<Guid> OnMarkAsRead { get; set; }
    [Parameter] public EventCallback<Guid> OnDelete { get; set; }
    [Parameter] public EventCallback<(string action, ButtonActionType actionType, Guid? invoiceId)> OnButtonAction { get; set; }
}
