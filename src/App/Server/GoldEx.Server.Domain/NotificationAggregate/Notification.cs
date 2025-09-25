using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.InvoiceAggregate;

namespace GoldEx.Server.Domain.NotificationAggregate;

public readonly record struct NotificationId(Guid Value);
public class Notification : EntityBase<NotificationId>
{
    public string Title { get; private set; }
    public string Message { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    public InvoiceId? InvoiceId { get; private set; }
    public Invoice? Invoice { get; private set; }

    public List<NotificationButton>? Buttons { get; set; }

#pragma warning disable CS8618 
    private Notification() { }
#pragma warning restore CS8618 

    private Notification(string title, string message, List<NotificationButton>? buttons = null, InvoiceId? invoiceId = null)
    {
        Id = new NotificationId(Guid.NewGuid());
        Title = title;
        Message = message;
        IsRead = false;
        InvoiceId = invoiceId;
        Buttons = buttons;
    }

    internal static Notification Create(string title,
        string message,
        List<NotificationButton>? buttons = null,
        InvoiceId? invoiceId = null) => new(title, message, buttons, invoiceId);

    public static Notification CreateInvoiceNotification(string title,
        string message,
        List<NotificationButton> buttons,
        InvoiceId invoiceId) => Create(title, message, buttons, invoiceId);

    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.Now;
    }
}