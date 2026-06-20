using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Domain.NotificationAggregate;

public readonly record struct NotificationId(Guid Value);
public class Notification : EntityBase<NotificationId>, IStoreFiltered
{
    public StoreId StoreId { get; private set; }
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

    private Notification(string title, string message, List<NotificationButton>? buttons = null, InvoiceId? invoiceId = null, StoreId storeId = default)
    {
        Id = new NotificationId(Guid.CreateVersion7());
        Title = title;
        Message = message;
        IsRead = false;
        InvoiceId = invoiceId;
        Buttons = buttons;
        StoreId = storeId;
    }

    internal static Notification Create(string title,
        string message,
        List<NotificationButton>? buttons = null,
        InvoiceId? invoiceId = null,
        StoreId storeId = default) => new(title, message, buttons, invoiceId, storeId);

    public static Notification CreateInvoiceNotification(string title,
        string message,
        List<NotificationButton> buttons,
        InvoiceId invoiceId,
        StoreId storeId = default) => Create(title, message, buttons, invoiceId, storeId);

    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.Now;
    }
}