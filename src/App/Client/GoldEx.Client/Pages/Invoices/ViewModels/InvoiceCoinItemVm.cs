using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Client.Pages.Settings.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoiceCoinItemVm
{
    private int _quantity;
    private decimal _unitPrice;
    private decimal _profitPercent;
    private CoinVm _coinVm = new();

    [Display(Name = "تعداد")]
    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            RecalculateAmounts();
        }
    }
}