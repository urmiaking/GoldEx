using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.FinancialAccounts.Components;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings;

public partial class FinancialAccounts
{
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false };
    private MudTable<FinancialAccountVm> _table = new();
    private string? _searchString;
    private FinancialAccountType? _accountType;
    public Color AccountTypeColor => _accountType switch
    {
        FinancialAccountType.Cash => Color.Info,
        FinancialAccountType.Gold => Color.Secondary,
        FinancialAccountType.LocalBankAccount => Color.Primary,
        FinancialAccountType.InternationalBankAccount => Color.Success,
        null => Color.Default,
        _ => throw new ArgumentOutOfRangeException()
    };
    public string? AccountTypeIcon => _accountType switch
    {
        FinancialAccountType.Cash => Icons.Material.Filled.AttachMoney,
        FinancialAccountType.Gold => Icons.Material.Filled._18UpRating,
        FinancialAccountType.LocalBankAccount => Icons.Material.Filled.AccountBalance,
        FinancialAccountType.InternationalBankAccount => Icons.Material.Filled.Public,
        null => Icons.Material.Filled.ViewHeadline,
        _ => throw new ArgumentOutOfRangeException()
    };

    [Inject] private HelpContext HelpContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        HelpContext.Slug = "financial-accounts";
        base.OnInitialized();
    }

    public override ValueTask DisposeAsync()
    {
        HelpContext.Slug = null;
        return base.DisposeAsync();
    }

    private async Task RefreshDataAsync()
    {
        await _table.ReloadServerData();
        StateHasChanged();
    }

    private async Task OnCreate()
    {
        var parameters = new DialogParameters<FinancialAccountEditor>
        {
            { x => x.SubmitIndependently, true },
            { x => x.IsSystemAccount, true }
        };

        var dialog = await DialogService.ShowAsync<FinancialAccountEditor>("افزودن حساب مالی جدید",
            parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("حساب مالی جدید با موفقیت افزوده شد.");
            await RefreshDataAsync();
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
            await RefreshDataAsync();
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
                afterSend: RefreshDataAsync);
        }
    }

    private async Task OnSearch(string text)
    {
        _searchString = text;

        if (_table.CurrentPage != 0)
            _table.NavigateTo(0);

        else
            await _table.ReloadServerData();
    }

    private void PageChanged(int i)
    {
        if (i <= 0)
            return;

        _table.NavigateTo(i - 1);
    }

    private async Task<TableData<FinancialAccountVm>> LoadFinancialAccounts(TableState state,
        CancellationToken cancellationToken = default)
    {
        var result = new TableData<FinancialAccountVm>();

        var financialAccountFilter = new FinancialAccountFilter(_accountType, true);

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        await SendRequestAsync<IFinancialAccountService, PagedList<GetFinancialAccountResponse>>(
            action: (service, token) => service.GetListAsync(filter, financialAccountFilter, token),
            afterSend: response =>
            {
                var items = response.Data.Select(FinancialAccountVm.CreateFrom).ToList();
                result = new TableData<FinancialAccountVm>
                {
                    TotalItems = response.Total,
                    Items = items
                };
            },
            createScope: true,
            cancelPrevious: true
        );

        return result;
    }

    private async Task SetAccountTypeFilterText(FinancialAccountType? accountType)
    {
        _accountType = accountType;
        await RefreshDataAsync();
    }
}