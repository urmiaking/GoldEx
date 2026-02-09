using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.LicensePaymentAggregate;

public readonly record struct LicensePaymentId(Guid Value);
public class LicensePayment : EntityBase<LicensePaymentId>
{
    public LicensePlan CurrentPlan { get; private set; }
    public LicensePlan RequestedPlan { get; private set; }
    public int RequestedMonths { get; private set; }
    public string PaymentReference { get; private set; }
    public string? PaymentDescription { get; private set; }
    public LicensePaymentStatus Status { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

#pragma warning disable CS8618
    public LicensePayment() {}
#pragma warning restore CS8618

    private LicensePayment(LicensePlan currentPlan, int requestedMonths, string paymentReference, string? paymentDescription)
    {
        int[] validMonths = [1, 3, 6, 12];

        if (!validMonths.Contains(requestedMonths))
            throw new InvalidOperationException("Invalid requested month");

        Id = new LicensePaymentId(Guid.CreateVersion7());
        CurrentPlan = currentPlan;
        RequestedPlan = LicensePlan.Regular;
        RequestedMonths = requestedMonths;
        PaymentReference = paymentReference;
        PaymentDescription = paymentDescription;
        Status = LicensePaymentStatus.Pending;
    }

    public static LicensePayment Create(LicensePlan currentPlan, int requestedMonths, string paymentReference, string? paymentDescription)
    {
        return new LicensePayment(currentPlan, requestedMonths, paymentReference, paymentDescription);
    }

    public void Approve()
    {
        Status = LicensePaymentStatus.Approved;
        ReviewedAt = DateTime.Now;
    }

    public void Reject()
    {
        Status = LicensePaymentStatus.Rejected;
        ReviewedAt = DateTime.Now;
    }
}