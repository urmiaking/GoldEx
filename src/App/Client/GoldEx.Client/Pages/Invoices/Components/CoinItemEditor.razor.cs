using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class CoinItemEditor
{
    [Parameter] public CoinItemVm Model { get; set; } = new() { Quantity = 1 };
    [Parameter] public GetPriceUnitTitleResponse? PriceUnit { get; set; }
    [Parameter] public InvoiceType InvoiceType { get; set; }
    [CascadingParameter] public IMudDialogInstance MudDialog { get; set; } = default!;

    private List<GetCoinResponse> _coins = [];
    private List<GetCustomerResponse> _customers = [];
    private MudForm _form = default!;
    private bool _isProcessing;
    private readonly CoinItemValidator _coinItemValidator = new();
    private int? _maxAvailableAmount;
    private MudColorPicker? _colorPicker;
    private readonly MudColor[] _colorPalette =
    [
        "#424242", "#2196f3", "#00c853", "#ff9800", "#f44336",
        "#f6f9fb", "#9df1fa", "#bdffcf", "#fff0a3", "#ffd254",
        "#e6e9eb", "#27dbf5", "#7ef7a0", "#ffe273", "#ffb31f",
        "#c9cccf", "#13b8e8", "#14dc71", "#fdd22f", "#ff9102",
        "#858791", "#0989c2", "#1bbd66", "#ebb323", "#fe6800",
        "#585b62", "#17698e", "#17a258", "#d9980d", "#dc3f11",
        "#353940", "#113b53", "#127942", "#bf7d11", "#aa0000",  
        Colors.Purple.Lighten5, Colors.Purple.Lighten4, Colors.Purple.Lighten3,
        Colors.Purple.Lighten2, Colors.Purple.Lighten1, Colors.Purple.Default,
        Colors.Purple.Darken1, Colors.Purple.Darken2, Colors.Purple.Darken3,
        Colors.Purple.Darken4, Colors.Purple.Accent1, Colors.Purple.Accent2,
        Colors.Purple.Accent3, Colors.Purple.Accent4
    ];

    protected override async Task OnParametersSetAsync()
    {
        if (Model.CoinInstance.Id.HasValue)
            await LoadMaxAmountAsync(Model.CoinInstance);

        await LoadCoinAsync();
        await base.OnParametersSetAsync();
    }

    private async Task LoadCoinAsync()
    {
        await SendRequestAsync<ICoinService, List<GetCoinResponse>>(
            action: (s, ct) => s.GetListAsync(true, ct),
            afterSend: response =>
            {
                _coins = response;
                StateHasChanged();
            });
    }

    private async Task LoadMaxAmountAsync(CoinInstanceVm coin)
    {
        if (coin.Id is null)
            return;

        await SendRequestAsync<IInventoryStockService, GetInventoryStockAmountResponse>(
            action: (s, ct) => s.GetAvailableItemAmountAsync(coin.Id.Value, ItemType.Coin, ct),
            afterSend: response => _maxAvailableAmount = (int)response.Amount);
    }

    public void Close() => MudDialog.Close();

    private async Task Submit()
    {
        _isProcessing = true;
        await _form.Validate();

        if (!_form.IsValid)
        {
            _isProcessing = false;
            return;
        }

        _isProcessing = false;
        MudDialog.Close(DialogResult.Ok(Model));
    }

    private async Task OnCoinChanged(GetCoinResponse? coin)
    {
        if (coin is null)
            return;

        Model.CoinInstance.Coin = coin;
        Model.CoinInstance.Weight = coin.Weight;
        Model.CoinInstance.Fineness = coin.Fineness;

        if (Model.CoinInstance.MintYear.HasValue)
        {
            if (Model.CoinInstance.MintYear.Value.Year < coin.StartMintYear ||
                (coin.EndMintYear.HasValue && Model.CoinInstance.MintYear.Value.Year > coin.EndMintYear.Value))
                Model.CoinInstance.MintYear = null;
        }

        if (PriceUnit is null)
            return;

        await SendRequestAsync<ICoinService, GetExchangeRateResponse?>(
            action: (s, ct) => s.GetPriceAsync(coin.Id, PriceUnit.IsDefault ? null : PriceUnit.Id, ct),
            afterSend: response =>
            {
                if (response is null)
                    return;

                Model.UnitPrice = response.ExchangeRate ?? 0;

                StateHasChanged();
            });
    }

    private int GetMaxAmount()
    {
        if (InvoiceType is InvoiceType.Purchase)
            return int.MaxValue;

        return _maxAvailableAmount ?? int.MaxValue;
    }

    private void OnCoinMintTypeChanged(CoinMintType coinMintType)
    {
        Model.CoinInstance.MintType = coinMintType;

        if (coinMintType is CoinMintType.Banking)
        {
            Model.CoinInstance.Weight = Model.CoinInstance.Weight;
            Model.CoinInstance.Fineness = Model.CoinInstance.Fineness;
        }
    }

    #region Customer

    private async Task<IEnumerable<CustomerVm>?> SearchCustomers(string? customerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            return null;

        await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
            action: (s, ct) => s.GetByNameAsync(customerName, CustomerType.Workshop, ct),
            afterSend: response =>
            {
                _customers = response;
            },
            cancelPrevious: true);

        return _customers.Select(CustomerVm.CreateFrom);
    }

    private async Task OnAddCustomer()
    {
        if (Model.CoinInstance.CoinPackage is null)
            return;

        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var parameters = new DialogParameters<Customers.Components.Editor>
        {
            { x => x.ReturnModel, true },
            { x => x.CustomerType, CustomerType.Workshop },
            { x => x.ShowFinancialAccounts, false }
        };

        var dialog = await DialogService.ShowAsync<Customers.Components.Editor>("افزودن کارگاه جدید", parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm customerVm })
        {
            Model.CoinInstance.CoinPackage.Issuer = customerVm;
            StateHasChanged();
        }
    }

    #endregion

    private async Task OpenColorPicker()
    {
        if (_colorPicker is not null)
            await _colorPicker.OpenAsync();
    }

    private DateTime? GetMinDate()
    {
        if (Model.CoinInstance.Coin is null)
            return null;

        if (Model.CoinInstance.Coin.StartMintYear is 0)
            return null;

        return new DateTime(Model.CoinInstance.Coin.StartMintYear, 3, 25);
    }

    private DateTime? GetMaxDate()
    {
        if (Model.CoinInstance.Coin is null)
            return null;

        if (Model.CoinInstance.Coin.EndMintYear is null or 0)
            return DateTime.Now;

        return new DateTime(Model.CoinInstance.Coin.EndMintYear.Value, 12, 31);
    }

    private void OnCoinPackageTypeChanged(CoinPackageType packageType)
    {
        Model.CoinInstance.PackageType = packageType;

        if (packageType is CoinPackageType.VacuumSealed && Model.CoinInstance.CoinPackage is null)
            Model.CoinInstance.CoinPackage = new CoinPackageSpecVm();
        else
            Model.CoinInstance.CoinPackage = null;
    }
}