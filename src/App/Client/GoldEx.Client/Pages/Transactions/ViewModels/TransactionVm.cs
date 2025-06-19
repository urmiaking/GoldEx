using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Transactions;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Transactions.ViewModels;

public class TransactionVm
{
    public Guid? TransactionId { get; set; }

    [Display(Name = "تاریخ تراکنش")]
    [Required(ErrorMessage = "{0} الزامی است")]
    public DateTime? TransactionDate { get; set; } = DateTime.Now;

    [Display(Name = "ساعت تراکنش")]
    [Required(ErrorMessage = "{0} الزامی است")]
    public TimeSpan? TransactionTime { get; set; } = DateTime.Now.TimeOfDay;

    [Display(Name = "شماره تراکنش")]
    [Required(ErrorMessage = "{0} الزامی است")]
    public long TransactionNumber { get; set; }

    [Display(Name = "شرح تراکنش")]
    [Required(ErrorMessage = "{0} الزامی است")]
    public string Description { get; set; } = default!;

    [Display(Name = "بدهکاری")]
    public decimal? Debit { get; set; }

    [Display(Name = "بستانکاری")]
    public decimal? Credit { get; set; }

    [Display(Name = "واحد بستانکاری")]
    public GetPriceUnitTitleResponse? CreditUnit { get; set; }

    [Display(Name = "واحد بدهکاری")]
    public GetPriceUnitTitleResponse? DebitUnit { get; set; }

    [Display(Name = "نرخ تبدیل بستانکاری")]
    public decimal? CreditRate { get; set; }

    [Display(Name = "نرخ تبدیل بدهکاری")]
    public decimal? DebitRate { get; set; }

    [Display(Name = "معادل ریالی بستانکاری")]
    public decimal? CreditEquivalent { get; set; }

    [Display(Name = "معادل ریالی بدهکاری")]
    public decimal? DebitEquivalent { get; set; }

    public CustomerVm Customer { get; set; } = new();

    public static TransactionVm CreateFrom(GetTransactionResponse transaction)
    {
        return new TransactionVm
        {
            Customer = CustomerVm.CreateFrom(transaction.Customer),
            TransactionId = transaction.Id,
            TransactionNumber = transaction.Number,
            Description = transaction.Description,
            TransactionDate = transaction.DateTime,
            TransactionTime = transaction.DateTime.TimeOfDay,
            Debit = transaction.Debit,
            DebitUnit = transaction.DebitPriceUnit,
            DebitRate = transaction.DebitRate,
            Credit = transaction.Credit,
            CreditUnit = transaction.CreditPriceUnit,
            CreditRate = transaction.CreditRate
        };
    }

    public static UpdateTransactionRequest ToUpdateRequest(TransactionVm model)
    {
        return new UpdateTransactionRequest(
            model.TransactionNumber,
            model.Description,
            DateTime.Today.Add(model.TransactionTime ?? TimeSpan.Zero),
            model.Credit,
            model.CreditUnit?.Id,
            model.CreditRate,
            model.Debit,
            model.DebitUnit?.Id,
            model.DebitRate,
            CustomerVm.ToRequest(model.Customer)
        );
    }

    public static CreateTransactionRequest ToCreateRequest(TransactionVm model)
    {
        return new CreateTransactionRequest(
            model.TransactionNumber,
            model.Description,
            DateTime.Today.Add(model.TransactionTime ?? TimeSpan.Zero),
            model.Credit,
            model.CreditUnit?.Id,
            model.CreditRate,
            model.Debit,
            model.DebitUnit?.Id,
            model.DebitRate,
            CustomerVm.ToRequest(model.Customer)
        );
    }
}