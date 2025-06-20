using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class Remove
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public long InvoiceNumber { get; set; }
    [Parameter] public Guid Id { get; set; }

    private async Task OnValidSubmit()
    {
        if (IsBusy)
            return;

        var deleteProducts = false;

        var result = await DialogService.ShowMessageBox(
            "هشدار",
            markupMessage: new MarkupString("با حذف فاکتور اجناس مربوط به آن به انبار بازگردد؟ <br> <br> "),
            yesText: "بله", noText: "خیر", cancelText: "لغو عملیات");

        if (result is null)
            return;

        if (result is false)
            deleteProducts = true;

        await SendRequestAsync<IInvoiceService>(
            action: (s, ct) => s.DeleteAsync(Id, deleteProducts, ct),
            afterSend: () =>
            {
                MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });

    }

    private void Cancel() => MudDialog.Cancel();
}