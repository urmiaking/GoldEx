using GoldEx.Client.Helpers;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Xml.Linq;

namespace GoldEx.Client.Pages.Products.Components;

public partial class ProductsList
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;

    private string _version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.1";

    private string? _searchString;
    private MudTable<ProductVm> _table = new ();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small};

    private async Task<TableData<ProductVm>> LoadProductsAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<ProductVm>();

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        await SendRequestAsync<IProductService, PagedList<GetProductResponse>>(
            action: (service, token) => service.GetListAsync(filter, token),
            afterSend: response =>
            {
                var items = response.Data.Select(ProductVm.CreateFrom).ToList();
                result = new TableData<ProductVm>
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

    public async Task OnCreateProduct()
    {
        var dialog = await DialogService.ShowAsync<Editor>("افزودن جنس جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("جنس جدید با موفقیت افزوده شد.");
            await _table.ReloadServerData();
        }
    }

    private async Task OnRemoveProduct(ProductVm model)
    {
        if (!model.Id.HasValue)
            return;

        var parameters = new DialogParameters<Remove>
        {
            { x => x.Id, model.Id.Value },
            { x => x.ProductName, model.Name }
        };

        var dialog = await DialogService.ShowAsync<Remove>("حذف جنس", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("جنس با موفقیت حذف شد.");
            await _table.ReloadServerData();
        }
    }

    private async Task OnEditProduct(ProductVm model)
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.Model, model }
        };

        var dialog = await DialogService.ShowAsync<Editor>("ویرایش جنس", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("جنس با موفقیت ویرایش شد.");
            await _table.ReloadServerData();
        }
    }

    private async Task OnPrintBarcode(ProductVm product)
    {
        var labelData = new
        {
            text = product.Barcode,
            name = product.Name,
            weight = "وزن: " + product.Weight?.ToString("G29") + "g",
            wage = "اجرت: " + product.WageType switch
            {
                WageType.Fixed => $"{product.Wage?.ToCurrencyFormat(product.WagePriceUnitTitle)}",
                WageType.Percent => product.Wage?.ToString("G29") + "%",
                _ => "ندارد"
            }
        };

        await JsRuntime.InvokeVoidAsync("printBarcode", labelData);
    }
}