using GoldEx.Client.Pages.Invoices.ViewModels;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class DiscountList
{
    [Parameter] public List<InvoiceDiscountVm> Items { get; set; } = [];

    protected override void OnParametersSet()
    {
        if (!Items.Any())
        {
            Items.Add(new InvoiceDiscountVm
            {
                Amount = 0,
                Description = string.Empty
            });
        }

        base.OnParametersSet();
    }

    private void AddItem()
    {
        Items.Add(new InvoiceDiscountVm
        {
            Amount = 0,
            Description = string.Empty
        });
    }

    private void RemoveItem(InvoiceDiscountVm item)
    {
        if (Items.Count > 1) 
            Items.Remove(item);
    }
}