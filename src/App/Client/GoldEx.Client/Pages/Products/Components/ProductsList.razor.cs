﻿using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Services;
using MudBlazor;

namespace GoldEx.Client.Pages.Products.Components;

public partial class ProductsList
{
    private string? _searchString;
    private MudTable<ProductVm> _table = new ();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false };

    private async Task<TableData<ProductVm>> LoadProductsAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<ProductVm>();
        using var scope = CreateServiceScope();
        var service = GetRequiredService<IProductClientService>(scope);

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

            var items = response.Data.Select(ProductVm.CreateFrom).ToList();

            result = new TableData<ProductVm>
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

    public async Task OnCreateProduct()
    {
        var dialog = await DialogService.ShowAsync<Create>("افزودن جنس جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("جنس جدید با موفقیت افزوده شد.");
            await _table.ReloadServerData();
        }
    }

    private Task OnRemoveProduct(ProductVm model)
    {
        throw new NotImplementedException();
    }

    private Task OnEditProduct(ProductVm model)
    {
        throw new NotImplementedException();
    }

    private Task OnRemoveSelectedProducts()
    {
        throw new NotImplementedException();
    }
}