using GoldEx.Client.Pages.Customers.Validators;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.FinancialAccounts.Components;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class Editor
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public CustomerVm Model { get; set; } = new();
    [Parameter] public bool ReturnModel { get; set; }

    private readonly DialogOptions _bankAccountsDialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };
    private readonly CustomerValidator _customerValidator = new();
    private MudForm _form = default!;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private string? _creditLimitAdornmentText;
    private bool _processing;

    protected override async Task OnParametersSetAsync()
    {
        await LoadPriceUnitsAsync();
        await base.OnParametersSetAsync();
    }

    private async Task LoadPriceUnitsAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: response =>
            {
                _priceUnits = response;

                if (Model.CreditLimitPriceUnit is null)
                {
                    var selectedUnit = _priceUnits.FirstOrDefault(u => u.IsDefault);
                    _creditLimitAdornmentText = selectedUnit?.Title;
                }
                else
                {
                    _creditLimitAdornmentText = Model.CreditLimitPriceUnit?.Title;
                }
            });
    }

    private async Task Submit()
    {
        if (_processing)
            return;

        _processing = true;

        await _form.Validate();

        if (!_form.IsValid)
            return;

        var request = CustomerVm.ToRequest(Model);

        if (!Model.Id.HasValue)
        {
            await SendRequestAsync<ICustomerService, Guid>(
                action: (s, ct) => s.CreateAsync(request, cancellationToken: ct),
                afterSend: async response =>
                {
                    if (ReturnModel)
                    {
                        await SendRequestAsync<ICustomerService, GetCustomerResponse>(
                            action: (s, ct) => s.GetAsync(response, ct),
                            afterSend: customerResponse => MudDialog.Close(DialogResult.Ok(CustomerVm.CreateFrom(customerResponse))));
                    }
                    else
                    {
                        MudDialog.Close(DialogResult.Ok(true));
                    }
                });
        }
        else
        {
            await SendRequestAsync<ICustomerService>(
                action: (s, ct) => s.UpdateAsync(Model.Id.Value, request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }

        _processing = false;
    }

    private void Close() => MudDialog.Cancel();

    private void OnCreditLimitUnitChanged(GetPriceUnitTitleResponse? unitType)
    {
        Model.CreditLimitPriceUnit = unitType;
    }

    private void SelectCreditLimitUnit(GetPriceUnitTitleResponse selectedUnit)
    {
        OnCreditLimitUnitChanged(selectedUnit);

        _creditLimitAdornmentText = selectedUnit.Title;
        Model.CreditLimitMenuOpen = false;
    }

    private async Task OnEditFinancialAccount(FinancialAccountVm financialAccount)
    {
        var parameters = new DialogParameters
        {
            { nameof(FinancialAccountEditor.Model), financialAccount },
            { nameof(FinancialAccountEditor.PriceUnits), _priceUnits }
        };
        var dialog = await DialogService.ShowAsync<FinancialAccountEditor>($"ویرایش حساب مالی {financialAccount.FinancialAccountType.GetDisplayName()}",
            parameters, _bankAccountsDialogOptions);
        await dialog.Result;
    }

    private async Task OnRemoveBankAccount(FinancialAccountVm financialAccount)
    {
        var result = await DialogService.ShowMessageBox(
            "هشدار",
            $"آیا برای حذف حساب مالی {financialAccount.FinancialAccountType.GetDisplayName()} مطمئن هستید؟",
            yesText: "بله", cancelText: "لغو");

        if (result is true)
        {
            Model.FinancialAccounts?.Remove(financialAccount);
            StateHasChanged();
        }
    }

    private async Task OnAddBankAccount()
    {
        var parameters = new DialogParameters
        {
            { nameof(FinancialAccountEditor.PriceUnits), _priceUnits },
            { nameof(FinancialAccountEditor.AccountHolderName), Model.FullName }
        };
        var dialog = await DialogService.ShowAsync<FinancialAccountEditor>("افزودن حساب مالی", parameters, _bankAccountsDialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: FinancialAccountVm bankAccount })
        {
            Model.FinancialAccounts ??= [];
            Model.FinancialAccounts.Add(bankAccount);
        }
    }
}