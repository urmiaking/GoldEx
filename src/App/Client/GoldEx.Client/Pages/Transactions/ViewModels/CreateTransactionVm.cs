using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Transactions.ViewModels;

public class CreateTransactionVm
{
    public DateTime? TransactionDate { get; set; } = DateTime.Now;
    public TimeSpan? TransactionTime { get; set; } = DateTime.Now.TimeOfDay;
    public int TransactionNumber { get; set; }
    public string CustomerNationalId { get; set; } = default!;
    public string CustomerFullName { get; set; } = default!;
    public string CustomerPhoneNumber { get; set; } = default!;
    public double? CustomerCreditLimit { get; set; }
    public UnitType? CustomerCreditLimitUnit { get; set; }
    public double? CustomerCreditRemaining { get; set; } = 200;
    public UnitType? CustomerCreditRemainingUnit { get; set; } = UnitType.USD;
    public string? CustomerAddress { get; set; }
    public CustomerType CustomerType { get; set; } = CustomerType.Individual;

    public string Description { get; set; } = default!;
    public double? Debit { get; set; }
    public double? Credit { get; set; }
    public UnitType? CreditUnit { get; set; }
    public UnitType? DebitUnit { get; set; }
    public double? CreditRate { get; set; }
    public double? DebitRate { get; set; }
    public double? CreditEquivalent { get; set; } = 100000;
    public double? DebitEquivalent { get; set; } = 200000000000;
}