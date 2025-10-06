using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Notifications;

public record GetNotificationResponse(Guid Id, string Title, string Message, List<NotificationButtonDto> Buttons, Guid? InvoiceId);

public record NotificationButtonDto(string Text, string Action, ButtonColor Color, ButtonActionType ActionType, ButtonIcon? ButtonIcon);