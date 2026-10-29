using GoldEx.Shared.DTOs.Notifications;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Components.Layouts.Components;

public partial class NotificationList
{
    [Parameter] public List<GetNotificationResponse> Items { get; set; } = [];
    [Parameter] public bool Busy { get; set; }
    [Parameter] public EventCallback<GetNotificationResponse> OnOpen { get; set; }
}
