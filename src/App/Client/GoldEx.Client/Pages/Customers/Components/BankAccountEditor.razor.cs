using GoldEx.Client.Pages.Customers.Validators;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class BankAccountEditor
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public BankAccountVm Model { get; set; } = new();
    [Parameter] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];

    private readonly BankAccountValidator _bankAccountValidator = new();
    private MudForm _form = default!;

    protected override void OnParametersSet()
    {
        Model.PriceUnit ??= PriceUnits.FirstOrDefault(x => x.IsDefault);

        base.OnParametersSet();
    }

    private void Close() => MudDialog.Cancel();

    private async Task Submit()
    {
        await _form.Validate();

        if (!_form.IsValid)
            return;

        MudDialog.Close(DialogResult.Ok(Model));
    }

    private void OnAccountTypeChanged(BankAccountType type)
    {
        Model.BankAccountType = type;

        switch (type)
        {
            case BankAccountType.Local:
                Model.InternationalAccountNumber = null;
                Model.SwiftBicCode = null;
                Model.IbanNumber = null;
                Model.PriceUnit = PriceUnits.FirstOrDefault(x => x.Title == "ریال");
                break;
            case BankAccountType.International:
                Model.LocalAccountNumber = null;
                Model.ShabaNumber = null;
                Model.CardNumber = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}