using System.ComponentModel.DataAnnotations;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoiceVm
{
    public Guid? InvoiceId { get; set; }

    [Display(Name = "شماره فاکتور")]
    public long InvoiceNumber { get; private set; }

    [Display(Name = "تاریخ سررسید")]
    public DateOnly? DueDate { get; private set; }

    [Display(Name = "تاریخ فاکتور")]
    public DateOnly InvoiceDate { get; private set; }

    public CustomerVm Customer { get; set; } = new();

    public List<InvoiceItemVm> InvoiceItems { get; set; } = [];
    public List<InvoiceDiscountVm> InvoiceDiscounts { get; set; } = [];
    public List<InvoiceExtraCostVm> InvoiceExtraCosts { get; set; } = [];
    public List<InvoicePaymentVm> InvoicePayments { get; set; } = [];

    public static InvoiceVm CreateDefaultInstance()
    {
        return new InvoiceVm
        {
            InvoiceDate = DateOnly.FromDateTime(DateTime.Now),
            Customer = CustomerVm.CreateDefaultInstance(),
            InvoiceItems = [],
            InvoiceDiscounts = [],
            InvoiceExtraCosts = [],
            InvoicePayments = []
        };
    }

    public void AddInvoiceItem(GetProductResponse response, decimal? gramPrice, decimal? exchangeRate, decimal? taxPercent, decimal? profitPercent)
    {
        var item = new InvoiceItemVm
        {
            Product = ProductVm.CreateFrom(response),
            Quantity = 1,
            GramPrice = gramPrice ?? 0,
            ExchangeRate = exchangeRate,
            ProfitPercent = profitPercent ?? 0,
            TaxPercent = taxPercent ?? 0
        };
        InvoiceItems.Add(item);
    }
}