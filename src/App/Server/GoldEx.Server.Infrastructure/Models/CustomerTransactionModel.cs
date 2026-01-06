using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Models;

public class CustomerTransactionModel
{
    public required DateTime PostingDate { get; set; } 
    public required string Description { get; set; }
    public required TransactionType TransactionType { get; set; }
    public required LedgerAccountRole Role { get; set; }
    public required decimal Amount { get; set; }
    public required decimal RunningBalance { get; set; }
    public required string PriceUnitTitle { get; set; }
    public decimal? ExchangeRate { get; set; }
    public required decimal BaseCurrencyAmount { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? InvoicePaymentId { get; set; }
    public Guid? PaymentVoucherId { get; set; }
}