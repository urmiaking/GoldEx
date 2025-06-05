using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Transactions;

namespace GoldEx.Client.Pages.Transactions.ViewModels;

public class TransactionVm
{
    public Guid Id { get; set; }

    public long TransactionNumber { get; set; }
    public string Description { get; set; } = default!;
    public string CustomerFullName { get; set; } = default!;
    public string CustomerNationalId { get; set; } = default!;
    public decimal? Credit { get; set; }
    public string? CreditUnit { get; set; }
    public decimal? Debit { get; set; }
    public string? DebitUnit { get; set; }
    //public decimal Remaining { get; set; }
    //public string RemainingUnit { get; set; } = default!;
    public DateTime DateTime { get; set; }

    public static TransactionVm CreateFrom(GetTransactionResponse response)
    {
        return new TransactionVm
        {
            Id = response.Id,
            TransactionNumber = response.Number,
            CustomerFullName = response.Customer.FullName,
            CustomerNationalId = response.Customer.NationalId,
            Credit = response.Credit,
            CreditUnit = response.CreditPriceUnit?.Title,
            Debit = response.Debit,
            DebitUnit = response.DebitPriceUnit?.Title,
            //Remaining = response.Credit ?? 0 - response.Debit ?? 0,
            //RemainingUnit = response.CreditUnit.HasValue
            //    ? response.CreditUnit.Value.GetDisplayName()
            //    : response.DebitUnit.HasValue
            //        ? response.DebitUnit.Value.GetDisplayName()
            //        : string.Empty,
            DateTime = response.DateTime,
            Description = response.Description
        };
    }
}