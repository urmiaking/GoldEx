using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.NotificationAggregate;

public class NotificationButton
{
    public string Text { get; set; } = default!;
    public string Action { get; set; } = default!;
    public ButtonColor Color { get; set; }
    public ButtonIcon? ButtonIcon { get; set; }
    public ButtonActionType ActionType { get; set; }

    public static NotificationButton CreateViewButton(string action) =>
        new()
        {
            Action = action,
            ButtonIcon = Shared.Enums.ButtonIcon.View,
            Color = ButtonColor.Success,
            ActionType = ButtonActionType.NavigateToUrl,
            Text = "مشاهده فاکتور"
        };

    public static NotificationButton CreateSendButton(string action) =>
        new()
        {
            Action = action,
            ButtonIcon = Shared.Enums.ButtonIcon.Send,
            Color = ButtonColor.Primary,
            ActionType = ButtonActionType.SendReminderSms,
            Text = "ارسال پیامک یادآوری"
        };
}