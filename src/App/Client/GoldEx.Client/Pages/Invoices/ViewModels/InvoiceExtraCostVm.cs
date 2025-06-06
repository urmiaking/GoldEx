using GoldEx.Shared.DTOs.PriceUnits;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoiceExtraCostVm
{
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public GetPriceUnitTitleResponse? PriceUnit { get; set; }
}