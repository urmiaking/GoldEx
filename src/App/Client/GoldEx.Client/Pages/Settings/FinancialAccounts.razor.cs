using GoldEx.Client.Pages.Customers.Components;
using GoldEx.Client.Pages.FinancialAccounts.Components;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.Services.Abstractions;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings;

public partial class FinancialAccounts
{
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false };
    private IEnumerable<FinancialAccountVm> _financialAccounts = new List<FinancialAccountVm>();
    private bool _processing;

    protected override async Task OnInitializedAsync()
    {
        await LoadFinancialAccountsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadFinancialAccountsAsync()
    {
        _processing = true;

        await SendRequestAsync<IFinancialAccountService, List<GetFinancialAccountResponse>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response =>
            {
                _financialAccounts = response.Select((item, index) =>
                {
                    var vm = FinancialAccountVm.CreateFrom(item);
                    vm.Index = index + 1;
                    return vm;
                });

                _processing = false;
            });
    }

    private async Task OnCreate()
    {
        var parameters = new DialogParameters<FinancialAccountEditor>
        {
            { x => x.SubmitIndependently, true },
            { x => x.IsSystemAccount, true }
        };

        var dialog = await DialogService.ShowAsync<FinancialAccountEditor>("افزودن حساب مالی جدید", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("حساب مالی جدید با موفقیت افزوده شد.");
            await LoadFinancialAccountsAsync();
        }
    }

    private async Task OnEdit(FinancialAccountVm model)
    {
        var parameters = new DialogParameters<FinancialAccountEditor>
        {
            { x => x.Model, model },
            { x => x.SubmitIndependently, true },
            { x => x.IsSystemAccount, true }
        };

        var dialog = await DialogService.ShowAsync<FinancialAccountEditor>("ویرایش حساب مالی", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("حساب مالی مورد نظر با موفقیت ویرایش شد.");
            await LoadFinancialAccountsAsync();
        }
    }

    private async Task OnRemove(FinancialAccountVm model)
    {
        if (!model.Id.HasValue)
            return;

        var result = await DialogService.ShowMessageBox(
            "هشدار",
            $"آیا برای حذف حساب مالی {model.FinancialAccountType.GetDisplayName()} مطمئن هستید؟",
            yesText: "بله", cancelText: "لغو");

        if (result is true)
        {
            await SendRequestAsync<IFinancialAccountService>(
                action: (s, ct) => s.DeleteAsync(model.Id.Value, ct),
                afterSend: LoadFinancialAccountsAsync);
        }
    }
}