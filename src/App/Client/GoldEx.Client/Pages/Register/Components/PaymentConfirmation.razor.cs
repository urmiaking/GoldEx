using GoldEx.Client.Pages.Register.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Register.Components;

public partial class PaymentConfirmation
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public ProductUpgradeVm Plan { get; set; } = default!;

    private MudForm form = default!;
    private bool isSubmitting;

    private PaymentInfoVm PaymentInfo { get; set; } = new();

    public class PaymentInfoVm
    {
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task Submit()
    {
        if (!form.IsValid)
            return;

        isSubmitting = true;
        StateHasChanged();

        try
        {
            // Simulate API call  
            await Task.Delay(1000);

            // Return payment info to parent  
            MudDialog.Close(DialogResult.Ok(PaymentInfo));
        }
        catch
        {
            isSubmitting = false;
            StateHasChanged();
        }
    }
}