using GoldEx.Client.Pages.FinancialAccounts.Validators;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Shared.DTOs.LedgerAccounts;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.FinancialAccounts.Components;

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
    [Parameter] public bool IsSystemAccount { get; set; }

    private readonly FinancialAccountValidator _financialAccountValidator = new();
    private MudForm _form = default!;
    private List<GetLedgerAccountResponse> _ledgerAccounts = [];

    protected override void OnParametersSet()
    {
        if (CustomerId.HasValue) 
            Model.CustomerId = CustomerId.Value;

        if (!string.IsNullOrEmpty(AccountHolderName))
            Model.HolderName = AccountHolderName;

        if (IsSystemAccount) 
            Model.IsSystemAccount = true;

        base.OnParametersSet();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!PriceUnits.Any()) 
            await LoadPriceUnitsAsync();

        Model.PriceUnit ??= PriceUnits.FirstOrDefault(x => x.IsDefault);

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
                Model.CashAccount = null;
                break;
            case FinancialAccountType.InternationalBankAccount:
                Model.InternationalBankAccount = new InternationalBankAccountVm();
                Model.LocalBankAccount = null;
                Model.CashAccount = null;
                break;
            case FinancialAccountType.Cash:
                Model.CashAccount = new CashAccountVm();
                Model.InternationalBankAccount = null;
                Model.LocalBankAccount = null;
                break;
            case FinancialAccountType.Gold:
                Model.CashAccount = null;
                Model.InternationalBankAccount = null;
                Model.LocalBankAccount = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        StateHasChanged();
    }

    private void OnCashAccountTypeChanged(CashAccountType cashAccountType)
    {
        if (Model.CashAccount != null)
            Model.CashAccount.AccountType = cashAccountType;

        if (cashAccountType is CashAccountType.Internal)
        {
            Model.HolderName = null;
            Model.BrokerName = null;
        }

        StateHasChanged();
    }
}