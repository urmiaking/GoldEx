using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Models;

public sealed class LedgerAccountTrialBalanceModel
{
    public string BasePriceUnitTitle { get; set; } = default!;
    public List<LedgerAccountTrialBalanceNodeModel> Nodes { get; set; } = [];
}

public sealed class LedgerAccountTrialBalanceNodeModel
{
    public Guid Id { get; set; }
    public Guid? ParentAccountId { get; set; }
    public string LedgerAccountTitle { get; set; } = default!;
    public LedgerAccountType LedgerAccountType { get; set; }
    public string BasePriceUnitTitle { get; set; } = default!;
    public decimal DebitAmountBase { get; set; }
    public decimal CreditAmountBase { get; set; }

    public List<LedgerAccountTrialBalanceNodeModel> SubLedgerAccounts { get; set; } = [];
}