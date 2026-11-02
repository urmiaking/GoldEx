using GoldEx.Client.Components.Components.Dialogs;
using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Customers.Validators;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.FinancialAccounts.Components;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class Editor
{
    [Inject] private ResponsiveDialogService ResponsiveDialogs { get; set; } = default!;
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public CustomerVm Model { get; set; } = new();
    [Parameter] public bool ReturnModel { get; set; }
    [Parameter] public CustomerType? CustomerType { get; set; }
    [Parameter] public bool ShowFinancialAccounts { get; set; } = true;

    private readonly DialogOptions _bankAccountsDialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };
    private readonly CustomerValidator _customerValidator = new();
    private MudForm _form = default!;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private bool _processing;
    private CustomerVm? _initializedModel;

    protected override async Task OnParametersSetAsync()
    {
        if (!ReferenceEquals(_initializedModel, Model))
        {
            _initializedModel = Model;
            await LoadPriceUnitsAsync();
            await PopulateCustomerNationalId();
        }
        await base.OnParametersSetAsync();
    }

    protected override void OnParametersSet()
    {
        if (Model.FinancialAccounts is not null)
            foreach (var modelFinancialAccount in Model.FinancialAccounts)
                modelFinancialAccount.CustomerId = Model.Id;

        if (CustomerType.HasValue)
            Model.CustomerType = CustomerType.Value;

        base.OnParametersSet();
    }

    private async Task LoadPriceUnitsAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: response => _priceUnits = response);
    }

    private async Task PopulateCustomerNationalId()
    {
        if (string.IsNullOrEmpty(Model.NationalId))
            await RenewNationalIdAsync();
    }

    private async Task Submit()
    {
        if (_processing)
            return;

        _processing = true;
        Session?.SetBusy(true);
        try
        {
            await _form.ValidateAsync();

            if (!_form.IsValid)
            {
                return;
            }

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
                                afterSend: customerResponse => Complete(CustomerVm.CreateFrom(customerResponse)));
                        }
                        else
                        {
                            Complete(true);
                        }
                    });
            }
            else
            {
                await SendRequestAsync<ICustomerService>(
                    action: (s, ct) => s.UpdateAsync(Model.Id.Value, request, ct),
                    afterSend: () =>
                    {
                        Complete(true);
                        return Task.CompletedTask;
                    });
            }
        }
        finally
        {
            _processing = false;
            Session?.SetBusy(false);
        }
    }

    private void Complete(object? data)
    {
        if (Session is not null) Session.Close(data);
        else MudDialog.Close(DialogResult.Ok(data));
    }

    private void Close()
    {
        if (_processing) return;
        if (Session is not null) Session.Cancel();
        else MudDialog.Cancel();
    }

    private async Task OnEditFinancialAccount(FinancialAccountVm financialAccount)
    {
        var parameters = new DialogParameters<FinancialAccountEditor>
        {
            { x => x.Model, Session is null ? financialAccount : EditorDraft.Copy(financialAccount) },
            { x => x.PriceUnits, _priceUnits },
            { x => x.CustomerId, Model.Id }
        };
        var result = await ShowAccountEditor($"ویرایش حساب مالی {financialAccount.FinancialAccountType.GetDisplayName()}", parameters);
        if (Session is not null && !result.Canceled && result.Data is FinancialAccountVm updated && Model.FinancialAccounts is not null)
        {
            var index = Model.FinancialAccounts.IndexOf(financialAccount);
            if (index >= 0) Model.FinancialAccounts[index] = updated;
        }
    }

    private async Task OnRemoveFinancialAccount(FinancialAccountVm financialAccount)
    {
        var message = $"آیا برای حذف حساب مالی {financialAccount.FinancialAccountType.GetDisplayName()} مطمئن هستید؟";
        bool confirmed;
        if (Session is not null)
        {
            var confirmation = ResponsiveDialogs.Show<ResponsiveConfirmation>("حذف حساب مالی", new() { [nameof(ResponsiveConfirmation.Message)] = message });
            var result = await confirmation.Result;
            confirmed = !result.Canceled && result.Data is true;
        }
        else
        {
            confirmed = await DialogService.ShowMessageBoxAsync("هشدار", message, yesText: "بله", cancelText: "لغو") is true;
        }
        if (confirmed) Model.FinancialAccounts?.Remove(financialAccount);
    }

    private async Task OnAddFinancialAccount()
    {
        var parameters = new DialogParameters<FinancialAccountEditor>
        {
            { x => x.PriceUnits, _priceUnits },
            { x => x.CustomerId, Model.Id },
            { x => x.AccountHolderName, Model.FullName }
        };
        var result = await ShowAccountEditor("افزودن حساب مالی", parameters);
        if (!result.Canceled && result.Data is FinancialAccountVm account)
        {
            Model.FinancialAccounts ??= [];
            Model.FinancialAccounts.Add(account);
        }
    }

    private async Task<ResponsiveDialogResult> ShowAccountEditor(string title, DialogParameters<FinancialAccountEditor> parameters)
    {
        if (Session is not null)
        {
            var dialog = ResponsiveDialogs.Show<FinancialAccountEditor>(title, parameters.ToDictionary(pair => pair.Key, pair => pair.Value!));
            return await dialog.Result;
        }
        var legacy = await DialogService.ShowAsync<FinancialAccountEditor>(title, parameters, _bankAccountsDialogOptions);
        var result = await legacy.Result;
        return new(result?.Canceled != false, result?.Data);
    }

    private async Task RenewNationalIdAsync()
    {
        await SendRequestAsync<ICustomerService, GetCustomerNationalIdResponse>(
            action: (s, ct) => s.GenerateNationalIdAsync(ct),
            afterSend: response => Model.NationalId = response.NationalId);
    } 
}
