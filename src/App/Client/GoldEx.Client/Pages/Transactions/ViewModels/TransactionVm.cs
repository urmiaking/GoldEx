using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Transactions;

namespace GoldEx.Client.Pages.Transactions.ViewModels;

public class TransactionVm
{
    public Guid Id { get; set; }

    public int TransactionNumber { get; set; }
    public string Description { get; set; } = default!;
    public string CustomerFullName { get; set; } = default!;
    public string CustomerNationalId { get; set; } = default!;
    public double? Credit { get; set; }
    public string? CreditUnit { get; set; }
    public double? Debit { get; set; }
    public string? DebitUnit { get; set; }
    public double Remaining { get; set; }
    public string RemainingUnit { get; set; } = default!;
    public DateTime DateTime { get; set; } = default!;

    public static TransactionVm CreateFrom(GetTransactionResponse response)
    {
        return new TransactionVm
        {
            Id = response.Id,
            TransactionNumber = response.Number,
            CustomerFullName = response.Customer.FullName,
            CustomerNationalId = response.Customer.NationalId,
            Credit = response.Credit,
            CreditUnit = response.CreditUnit?.GetDisplayName(),
            Debit = response.Debit,
            DebitUnit = response.DebitUnit?.GetDisplayName(),
            Remaining = response.Credit ?? 0 - response.Debit ?? 0,
            RemainingUnit = response.CreditUnit.HasValue
                ? response.CreditUnit.Value.GetDisplayName()
                : response.DebitUnit.HasValue
                    ? response.DebitUnit.Value.GetDisplayName()
                    : string.Empty,
            DateTime = response.DateTime,
            Description = response.Description
        };
    }
}