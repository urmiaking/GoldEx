using GoldEx.Shared.DTOs.Notifications;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using System;

namespace GoldEx.Client.Components.Layouts.Components;

public partial class Notifications
{
    private List<GetNotificationResponse> _notifications = [];
    private bool _isOpen;
    private int _unreadCount;

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen == value) return;

            _isOpen = value;

            if (_isOpen)
            {
                _ = LoadNotificationsAsync();
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadNotificationsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadNotificationsAsync()
    {
        await SendRequestAsync<INotificationService, List<GetNotificationResponse>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response =>
            {
                _notifications = response;
                _unreadCount = response.Count;
            });
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