using GoldEx.Client.Pages.Invoices.ViewModels;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class ExtraCostList
{
    [Parameter] public List<InvoiceExtraCostVm> Items { get; set; } = [];

    protected override void OnParametersSet()
    {
        if (!Items.Any())
        {
            Items.Add(new InvoiceExtraCostVm
            {
                Amount = 0,
                Description = string.Empty
            });
        }

        base.OnParametersSet();
    }

    private void AddItem()
    {
        Items.Add(new InvoiceExtraCostVm
        {
            Amount = 0,
            Description = string.Empty
        });
    }

    private void RemoveItem(InvoiceExtraCostVm item)
    {
        if (Items.Count > 1)
            Items.Remove(item);
    }
}