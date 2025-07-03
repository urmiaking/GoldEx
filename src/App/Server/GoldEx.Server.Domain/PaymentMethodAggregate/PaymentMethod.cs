using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.PaymentMethodAggregate;

public readonly record struct PaymentMethodId(Guid Value);
public class PaymentMethod : EntityBase<PaymentMethodId>
{
    public static PaymentMethod Create(string title)
    {
        return new PaymentMethod
        {
            Id = new PaymentMethodId(Guid.NewGuid()),
            Title = title,
            IsActive = true
        };
    }

#pragma warning disable CS8618
    private PaymentMethod() { }
#pragma warning restore CS8618 

    public string Title { get; private set; }
    public bool IsActive { get; set; }

    public void SetTitle(string title) => Title = title;
    public void SetStatus(bool isActive) => IsActive = isActive;
}