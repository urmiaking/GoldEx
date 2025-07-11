﻿using GoldEx.Client.Pages.Transactions.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Transactions.Components;

public partial class TransactionsList
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 0;

    [Parameter] public Guid? CustomerId { get; set; }

    private string? _searchString;
    private MudTable<TransactionVm> _table = new();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Large, BackdropClick = false };
    private readonly DialogOptions _removeDialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small, BackdropClick = false };

    protected override async Task OnParametersSetAsync()
    {
        await _table.ReloadServerData();
        await base.OnParametersSetAsync();
    }

    private async Task<TableData<TransactionVm>> LoadTransactionsAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<TransactionVm>();

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        await SendRequestAsync<ITransactionService, PagedList<GetTransactionResponse>>(
            action: (s, ct) => s.GetListAsync(filter, CustomerId, ct),
            afterSend: response =>
            {
                var items = response.Data.Select(TransactionVm.CreateFrom).ToList();

                result = new TableData<TransactionVm>
                {
                    TotalItems = response.Total,
                    Items = items
                };
            }
        );

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

    public async Task OnCreateTransaction()
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.CustomerId, CustomerId }
        };

        var dialog = await DialogService.ShowAsync<Editor>("افزودن تراکنش جدید", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("تراکنش جدید با موفقیت افزوده شد.");
            await _table.ReloadServerData();
        }
    }

    private async Task OnRemoveTransaction(TransactionVm model)
    {
        if (!model.TransactionId.HasValue)
            return;

        var parameters = new DialogParameters<Remove>
        {
            { x => x.Id, model.TransactionId.Value },
            { x => x.TransactionNumber, model.TransactionNumber }
        };

        var dialog = await DialogService.ShowAsync<Remove>("حذف تراکنش", parameters, _removeDialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("تراکنش با موفقیت حذف شد.");
            await _table.ReloadServerData();
        }
    }

    private async Task OnEditTransaction(TransactionVm model)
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.Model, model }
        };

        var dialog = await DialogService.ShowAsync<Editor>("ویرایش تراکنش", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("تراکنش با موفقیت ویرایش شد.");
            await _table.ReloadServerData();
        }
    }
}