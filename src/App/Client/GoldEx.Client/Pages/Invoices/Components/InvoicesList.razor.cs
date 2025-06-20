using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class InvoicesList
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 0;
    [Parameter] public Guid? CustomerId { get; set; }
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;

    private string? _searchString;
    private MudTable<InvoiceListVm> _table = new();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

    private async Task<TableData<InvoiceListVm>> LoadInvoicesAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<InvoiceListVm>();

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        await SendRequestAsync<IInvoiceService, PagedList<GetInvoiceResponse>>(
            action: (service, token) => service.GetListAsync(filter, CustomerId, token),
            afterSend: response =>
            {
                var items = response.Data.Select(InvoiceListVm.CreateFrom).ToList();
                result = new TableData<InvoiceListVm>
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

    public async Task OnCreateInvoice()
    {
        NavigationManager.NavigateTo(ClientRoutes.Invoices.Create);
    }

    private async Task OnRemoveInvoice(InvoiceListVm model)
    {
        var parameters = new DialogParameters<Remove>
        {
            { x => x.Id, model.Id },
            { x => x.InvoiceNumber, model.InvoiceNumber }
        };

        var dialog = await DialogService.ShowAsync<Remove>("حذف فاکتور", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("فاکتور با موفقیت حذف شد.");
            await _table.ReloadServerData();
        }
    }

    private async Task OnEditInvoice(InvoiceListVm model)
    {
        //var parameters = new DialogParameters<Editor>
        //{
        //    { x => x.Model, model }
        //};

        //var dialog = await DialogService.ShowAsync<Editor>("ویرایش فاکتور", parameters, _dialogOptions);

        //var result = await dialog.Result;

        //if (result is { Canceled: false })
        //{
        //    AddSuccessToast("فاکتور با موفقیت ویرایش شد.");
        //    await _table.ReloadServerData();
        //}
    }
}