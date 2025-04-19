using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Transactions.Components;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class CustomersList
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 0;

    private string? _searchString;
    private MudTable<CustomerVm> _table = new();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };
    private readonly DialogOptions _viewTransactionDialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Large };

    private async Task<TableData<CustomerVm>> LoadCustomersAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<CustomerVm>();
        using var scope = CreateServiceScope();
        var service = GetRequiredService<ICustomerClientService>(scope);

        try
        {
            SetBusy();
            CancelToken();

            var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
                state.SortDirection switch
                {
                    SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                    SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                    SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                    _ => throw new ArgumentOutOfRangeException()
                });

            var response = await service.GetListAsync(filter, cancellationToken);

            var items = response.Data.Select(CustomerVm.CreateFrom).ToList();

            result = new TableData<CustomerVm>
            {
                TotalItems = response.Total,
                Items = items
            };
        }
        catch (Exception ex)
        {
            AddExceptionToast(ex);
        }
        finally
        {
            SetIdeal();
        }

        return result;
    }

    private async Task OnSearch(string text)
    {
        _searchString = text;
        await _table.ReloadServerData();
    }
        
    private void PageChanged(int i)
    {
        _table.NavigateTo(i - 1);   
    }

    public async Task OnCreate()
    {
        var dialog = await DialogService.ShowAsync<Create>("افزودن مشتری جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("مشتری جدید با موفقیت افزوده شد.");
            await _table.ReloadServerData();
        }
    }

    private async Task OnRemove(CustomerVm model)
    {
        var parameters = new DialogParameters<Remove>
        {
            { x => x.Id, model.Id },
            { x => x.CustomerName, model.FullName }
        };

        var dialog = await DialogService.ShowAsync<Remove>("حذف مشتری", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("مشتری با موفقیت حذف شد.");
            await _table.ReloadServerData();
        }
    }

    private async Task OnEdit(CustomerVm model)
    {
        var parameters = new DialogParameters<Update>
        {
            { x => x.Model, model }
        };

        var dialog = await DialogService.ShowAsync<Update>("ویرایش اطلاعات مشتری", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("اطلاعات مشتری با موفقیت ویرایش شد.");
            await _table.ReloadServerData();
        }
    }

    private async Task OnViewTransaction(CustomerVm customerVm)
    {
        var parameters = new DialogParameters
        {
            { nameof(TransactionsList.CustomerId), customerVm.Id },
            { nameof(TransactionsList.CustomerName), customerVm.FullName }
        };
        var dialog = await DialogService.ShowAsync<TransactionsList>($"تراکنش های {customerVm.FullName}", parameters, _viewTransactionDialogOptions);
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            AddSuccessToast("اطلاعات مشتری با موفقیت ویرایش شد.");
            await _table.ReloadServerData();
        }
    }
}