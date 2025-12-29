using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Settings.Components.LedgerAccounts;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.LedgerAccounts;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings;

public partial class LedgerAccounts
{
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false };
    private IReadOnlyCollection<TreeItemData<GetLedgerAccountResponse>> _treeItems = [];

    [Inject] private HelpContext HelpContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        HelpContext.Slug = "ledger-accounts";
        base.OnInitialized();
    }

    public override ValueTask DisposeAsync()
    {
        HelpContext.Slug = null;
        return base.DisposeAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadLedgerAccountsAsync();
        await base.OnParametersSetAsync();
    }

    private async Task LoadLedgerAccountsAsync()
    {
        await SendRequestAsync<ILedgerAccountService, List<GetLedgerAccountResponse>>(
            action: (s, ct) => s.GetListAsync(null, ct),
            afterSend: response =>
            {
                _treeItems = BuildTreeRecursively(response, null);

                StateHasChanged();
            });
    }

    private List<TreeItemData<GetLedgerAccountResponse>> BuildTreeRecursively(
        List<GetLedgerAccountResponse> allAccounts,
        Guid? parentId)
    {
        return allAccounts
            .Where(account => account.ParentAccount?.Id == parentId)
            .Select(account => new TreeItemData<GetLedgerAccountResponse>
            {
                Value = account,
                Text = account.Title,
                Children = BuildTreeRecursively(allAccounts, account.Id)
            })
            .ToList();
    }

    private async Task OnCreate(GetLedgerAccountResponse? parentItem = null)
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.Model, new LedgerAccountVm
                {
                    AccountType = parentItem?.AccountType,
                    ParentAccountId = parentItem?.Id
                }
            }
        };

        var dialog = await DialogService.ShowAsync<Editor>(parentItem is not null
                ? $"افزودن زیرسرفصل به {parentItem.Title}"
                : "افزودن سرفصل جدید",
            parameters,
            _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("سرفصل جدید با موفقیت افزوده شد");
            await LoadLedgerAccountsAsync();
        }
    }

    private Task OnEdit(GetLedgerAccountResponse? selectedItem)
    {
        AddErrorToast($"ویرایش سرفصل {selectedItem?.Title} در حال حاضر امکان پذیر نمی باشد");
        return Task.CompletedTask;

        //if (selectedItem == null)
        //    return;

        //var parameters = new DialogParameters<Editor>
        //{
        //    { x => x.Model, LedgerAccountVm.CreateFrom(selectedItem) }
        //};

        //var dialog = await DialogService.ShowAsync<Editor>(
        //    $"ویرایش سرفصل {selectedItem.Title}",
        //    parameters,
        //    _dialogOptions);

        //var result = await dialog.Result;

        //if (result is { Canceled: false })
        //{
        //    AddSuccessToast("سرفصل با موفقیت ویرایش شد");
        //    await LoadLedgerAccountsAsync();
        //}
    }

    private Task OnRemove(GetLedgerAccountResponse? selectedItem)
    {
        AddErrorToast($"حذف سرفصل {selectedItem?.Title} در حال حاضر امکان پذیر نمی باشد");
        return Task.CompletedTask;

        //if (selectedItem == null)
        //{
        //    return;
        //}
        //var result = await DialogService.ShowMessageBox(
        //    "حذف سرفصل",
        //    $"آیا از حذف سرفصل '{selectedItem.Title}' مطمئن هستید؟",
        //    yesText: "حذف",
        //    cancelText: "لغو",
        //    options: _dialogOptions);

        //if (result is true)
        //{
        //    await SendRequestAsync<ILedgerAccountService>(
        //        action: (s, ct) => s.DeleteAsync(selectedItem.Id, ct),
        //        afterSend: () =>
        //        {
        //            AddSuccessToast("سرفصل با موفقیت حذف شد");
        //            return LoadLedgerAccountsAsync();
        //        });
        //}
    }
}