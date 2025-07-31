using GoldEx.Client.Pages.Customers.Validators;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class FinancialAccountEditor
{
    [Parameter] public string? AccountHolderName { get; set; }
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public FinancialAccountVm Model { get; set; } = new()
    {
        FinancialAccountType = FinancialAccountType.LocalBankAccount,
        LocalBankAccount = new LocalBankAccountVm()
    };

    [Parameter] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];
    [Parameter] public bool SubmitIndependently { get; set; }
    [Parameter] public Guid? CustomerId { get; set; }

    private readonly FinancialAccountValidator _financialAccountValidator = new();
    private MudForm _form = default!;

    protected override void OnParametersSet()
    {
        Model.PriceUnit ??= PriceUnits.FirstOrDefault(x => x.IsDefault);

        if (CustomerId.HasValue) 
            Model.CustomerId = CustomerId.Value;

        if (Model.LocalBankAccount != null && !string.IsNullOrEmpty(AccountHolderName))
            Model.LocalBankAccount.AccountHolderName = AccountHolderName;

        base.OnParametersSet();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!PriceUnits.Any()) 
            await LoadPriceUnitsAsync();

        await base.OnParametersSetAsync();
    }

    private async Task LoadPriceUnitsAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: response => PriceUnits = response);
    }   

    private void Close() => MudDialog.Cancel();

    private async Task Submit()
    {
        await _form.Validate();

        if (!_form.IsValid)
            return;

        if (SubmitIndependently)
        {
            if (Model.Id.HasValue)
                await SendRequestAsync<IFinancialAccountService>(
                    action: (s, ct) => s.UpdateAsync(Model.Id.Value, Model.ToRequest(), ct),
                    afterSend: () =>
                    {
                        MudDialog.Close(DialogResult.Ok(Model));
                        return Task.CompletedTask;
                    });
            else
                await SendRequestAsync<IFinancialAccountService>(
                    action: (s, ct) => s.CreateAsync(Model.ToRequest(), ct),
                    afterSend: () =>
                    {
                        MudDialog.Close(DialogResult.Ok(Model));
                        return Task.CompletedTask;
                    });
        }
        else
        {
            MudDialog.Close(DialogResult.Ok(Model));
        }
    }

    private void OnAccountTypeChanged(FinancialAccountType type)
    {
        Model.FinancialAccountType = type;

        switch (type)
        {
            case FinancialAccountType.LocalBankAccount:
                Model.LocalBankAccount = new LocalBankAccountVm();
                Model.InternationalBankAccount = null;
                break;
            case FinancialAccountType.InternationalBankAccount:
                Model.InternationalBankAccount = new InternationalBankAccountVm();
                Model.LocalBankAccount = null;
                break;
            case FinancialAccountType.Cash:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        StateHasChanged();
    }
}