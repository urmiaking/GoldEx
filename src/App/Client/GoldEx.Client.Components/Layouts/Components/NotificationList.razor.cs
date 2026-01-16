using GoldEx.Shared.DTOs.Notifications;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace GoldEx.Client.Components.Layouts.Components;

public partial class NotificationList
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public List<GetNotificationResponse> UnreadItems { get; set; } = [];
    [Parameter] public List<GetNotificationResponse> ReadItems { get; set; } = [];
    [Parameter] public EventCallback<Guid> OnMarkAsRead { get; set; }
    [Parameter] public EventCallback<Guid> OnDelete { get; set; }
    [Parameter] public EventCallback OnMarkAllAsRead { get; set; }
    [Parameter] public EventCallback<(string action, ButtonActionType actionType, Guid? invoiceId)> OnButtonAction { get; set; }

    private readonly Dictionary<Guid, TouchInfo> _touchStates = new();

    private class TouchInfo
    {
        public double StartX { get; set; }
        public double CurrentX { get; set; }
        public bool IsSwipeActive { get; set; }
    }

    private void HandleTouchStart(TouchEventArgs e, Guid notificationId)
    {
        if (e.Touches.Length > 0)
        {
            _touchStates[notificationId] = new TouchInfo
            {
                StartX = e.Touches[0].ClientX,
                CurrentX = e.Touches[0].ClientX,
                IsSwipeActive = true
            };
        }
    }

    private void HandleTouchMove(TouchEventArgs e, Guid notificationId)
    {
        if (e.Touches.Length > 0 && _touchStates.TryGetValue(notificationId, out var touchInfo))
        {
            touchInfo.CurrentX = e.Touches[0].ClientX;

            var swipeDistance = touchInfo.CurrentX - touchInfo.StartX;

            if (Math.Abs(swipeDistance) > 50) 
            {
                // Add visual feedback here if needed  
            }
        }
    }

    private async Task HandleTouchEnd(TouchEventArgs e, Guid notificationId)
    {
        if (_touchStates.ContainsKey(notificationId))
        {
            var touchInfo = _touchStates[notificationId];
            var swipeDistance = touchInfo.CurrentX - touchInfo.StartX;

            if (swipeDistance > 100)
            {
                await OnMarkAsRead.InvokeAsync(notificationId);
            }

            _touchStates.Remove(notificationId);
        }
    }

    private Color ConvertButtonColor(ButtonColor color) => color switch
    {
        ButtonColor.Primary => Color.Primary,
        ButtonColor.Secondary => Color.Secondary,
        ButtonColor.Info => Color.Info,
        ButtonColor.Error => Color.Error,
        ButtonColor.Success => Color.Success,
        _ => Color.Default
    };

    private string ConvertButtonIcon(ButtonIcon? icon) => icon switch
    {
        ButtonIcon.View => Icons.Material.Filled.Visibility,
        ButtonIcon.Send => Icons.Material.Filled.Send,
        ButtonIcon.Edit => Icons.Material.Filled.Edit,
        ButtonIcon.Delete => Icons.Material.Filled.Delete,
        _ => string.Empty
    };

    private async Task HandleButtonClick((string action, ButtonActionType actionType, Guid? invoiceId) tuple)
    {
        await OnButtonAction.InvokeAsync(tuple);
    }
}