using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Settings.Components.Stores;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Stores;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Client.Pages.Settings;

public partial class Stores
{
    private MudTable<StoreVm> _table = new();
    private string? _searchString;
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, MaxWidth = MaxWidth.Small };

    [Inject] private HelpContext HelpContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        HelpContext.Slug = "stores";
        base.OnInitialized();
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

    private async Task<TableData<StoreVm>> LoadStoresAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<StoreVm>();

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        await SendRequestAsync<IStoreService, PagedList<GetStoreRequest>>(
            action: (service, token) => service.GetListAsync(filter, token),
            afterSend: response =>
            {
                result = new TableData<StoreVm>
                {
                    TotalItems = response.Total,
                    Items = response.Data.Select((item, idx) => new StoreVm
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Slug = item.Slug,
                        LogoUrl = item.LogoUrl,
                        BackgroundImageUrl = item.BackgroundImageUrl,
                        IsActive = item.IsActive,
                        Index = filter.Skip ?? 0 + idx + 1
                    }).ToList()
                };
            },
            createScope: true,
            cancelPrevious: true
        );

        return result;
    }

    private async Task OnCreateStore()
    {
        var dialog = await DialogService.ShowAsync<Editor>("افزودن فروشگاه جدید", _dialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("فروشگاه جدید با موفقیت ایجاد شد.");
            await _table.ReloadServerData();
        }
    }

    private async Task OnEditStore(StoreVm model)
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.Model, model },
            { x => x.Id, model.Id }
        };

        var dialog = await DialogService.ShowAsync<Editor>("ویرایش فروشگاه", parameters, _dialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("فروشگاه مورد نظر با موفقیت ویرایش شد.");
            await _table.ReloadServerData();
        }
    }

    private async Task OnRemoveStore(StoreVm model)
    {
        var parameters = new DialogParameters<Remove>
        {
            { x => x.Id, model.Id },
            { x => x.StoreName, model.Name }
        };

        var dialog = await DialogService.ShowAsync<Remove>("حذف فروشگاه", parameters, _dialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast($"فروشگاه {model.Name} با موفقیت حذف شد.");
            await _table.ReloadServerData();
        }
    }

    private async Task OnAssignUsers(StoreVm model)
    {
        var parameters = new DialogParameters<StoreUsersDialog>
        {
            { x => x.StoreId, model.Id },
            { x => x.StoreName, model.Name }
        };

        var options = new DialogOptions { CloseButton = true, FullWidth = true, MaxWidth = MaxWidth.Medium };
        var dialog = await DialogService.ShowAsync<StoreUsersDialog>("تخصیص کاربران به فروشگاه", parameters, options);
        await dialog.Result;
    }
}
