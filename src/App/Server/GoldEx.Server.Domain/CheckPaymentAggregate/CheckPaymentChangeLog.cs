using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.CheckPaymentAggregate;

public class CheckPaymentChangeLog : EntityBase
{
    public DateTime DateTime { get; private set; }
    public string? Description { get; private set; }
    public CheckPaymentStatus Status { get; private set; }
    public FinancialAccountId? TargetFinancialAccountId { get; private set; }
    public FinancialAccount? TargetFinancialAccount { get; private set; }

    private CheckPaymentChangeLog(CheckPaymentStatus status, string? description, FinancialAccountId? targetFinancialAccountId)
    {
        DateTime = DateTime.Now;
        Description = description;
        Status = status;
        TargetFinancialAccountId = targetFinancialAccountId;
    }

    public static CheckPaymentChangeLog Create(CheckPaymentStatus status, string? description = null, FinancialAccountId? targetFinancialAccountId = null)
    {
        return new CheckPaymentChangeLog(status, description, targetFinancialAccountId);
    }
}