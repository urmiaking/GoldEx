using GoldEx.Shared.DTOs.Notifications;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Client.Components.Layouts.Components;

public partial class Notifications
{
    private List<GetNotificationResponse> _unreadNotifications = [];
    private List<GetNotificationResponse> _readNotifications = [];
    private bool _drawerOpen;
    private int _unreadCount;

    protected override async Task OnInitializedAsync()
    {
        await LoadNotificationsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadNotificationsAsync()
    {
        await SendRequestAsync<INotificationService, List<GetNotificationResponse>>(
            action: (s, ct) => s.GetListAsync(null, ct),
            afterSend: response =>
            {
                _unreadNotifications = response.Where(x => !x.IsRead).ToList();
                _readNotifications = response.Where(x => x.IsRead).ToList();
                _unreadCount = response.Count;
            });
    }

    private async Task HandleDelete(Guid id)
    {
        await SendRequestAsync<INotificationService>(
            action: (s, ct) => s.DeleteAsync(id, ct),
            afterSend: LoadNotificationsAsync);
    }

    private async Task HandleMarkAsRead(Guid id)
    {
        await SendRequestAsync<INotificationService>(
            action: (s, ct) => s.MarkAsReadAsync(id, ct),
            afterSend: LoadNotificationsAsync);
    }

    private async Task HandleMarkAllAsRead()
    {
        await SendRequestAsync<INotificationService>(
            action: (s, ct) => s.MarkAllAsReadAsync(ct),
            afterSend: LoadNotificationsAsync);
    }

    private async Task HandleButtonAction((string action, ButtonActionType actionType, Guid? invoiceId) tuple)
    {
        switch (tuple.actionType)
        {
            case ButtonActionType.NavigateToUrl:
                Navigation.NavigateTo(tuple.action);
                break;
            case ButtonActionType.SendReminderSms:
                if (!tuple.invoiceId.HasValue)
                {
                    AddErrorToast("خطایی در ارسال پیامک رخ داد");
                    return;
                }

                await SendRequestAsync<IInvoiceService>(
                    action: (s, ct) => s.SendReminderAsync(tuple.invoiceId.Value, ct),
                    afterSend: () =>
                    {
                        AddSuccessToast("پیامک با موفقیت ارسال شد");
                        return Task.CompletedTask;
                    });
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}