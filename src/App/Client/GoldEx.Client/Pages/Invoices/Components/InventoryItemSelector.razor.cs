using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class InventoryItemSelector
{
    [Parameter] public ItemType ItemType { get; set; }
    [Parameter] public ItemStatus ItemStatus { get; set; }
    [Parameter] public decimal GramPrice { get; set; }
    [Parameter] public decimal TaxPercent { get; set; }
    [Parameter] public decimal GoldProfitPercent { get; set; }
    [Parameter] public decimal JewelryProfitPercent { get; set; }
    [Parameter] public ItemType[] SelectableTypes { get; set; } = [];
    [Parameter, EditorRequired] public GetPriceUnitTitleResponse PriceUnit { get; set; } = null!;
    [CascadingParameter] public IMudDialogInstance Dialog { get; set; } = default!;

    private List<ProductVm>? _products;
    private List<CoinVm>? _coins;
    private List<PriceUnitVm>? _currencies;

    private List<InventoryStockVm>? _selectedItems;

    private void OnSelectedItemsChanged(HashSet<InventoryStockVm>? items)
    {
        if (items is null)
            return;

        _selectedItems = items.ToList();

        if (items.All(x => x.Product is not null))
        {
            _products = items.Select(x => x.Product!).ToList();
        }
        else if (items.All(x => x.Coin is not null))
        {
            _coins = items.Select(x => x.Coin!).ToList();
        }
        else if (items.All(x => x.Currency is not null))
        {
            _currencies = items.Select(x => x.Currency!).ToList();
        }
    }

    public async Task OnSubmit()
    {
        if (_products is not null)
        {
            var productItemTasks = _products.Select(async product =>
            {
                var stock = _selectedItems?.FirstOrDefault(x => x.Product?.Id == product.Id);

                var stonePriceUnitExchangeRate = await GetCurrencyPriceAsync(product.StonePriceUnit?.Id ?? PriceUnit.Id);
                var wagePriceUnitExchangeRate = await GetCurrencyPriceAsync(product.WagePriceUnitId ?? PriceUnit.Id);

                return new ProductItemVm
                {
                    GramPrice = GramPrice,
                    TotalWeight = stock?.CurrentAmount,
                    TaxPercent = TaxPercent,
                    ProfitPercent = product.ProductType is ProductType.Jewelry
                        ? JewelryProfitPercent
                        : GoldProfitPercent,
                    Product = product,
                    Quantity = 1,
                    InvoiceType = InvoiceType.Sell,
                    StonePriceUnitExchangeRate = stonePriceUnitExchangeRate,
                    WageExchangeRate = wagePriceUnitExchangeRate
                }.RecalculateAmounts();
            });

            var productItems = (await Task.WhenAll(productItemTasks)).ToList();

            Dialog.Close(productItems);
        }

        else if (_coins is not null)
        {
            var coinItemTasks = _coins.Select(async coin =>
            {
                var unitPrice = await GetCoinPriceAsync(coin.Id!.Value);

                return await new CoinItemVm
                {
                    Coin = new GetCoinResponse(coin.Id!.Value,
                        coin.Title!,
                        coin.IsActive,
                        coin.PriceId!.Value),
                    Quantity = 1,
                    UnitPrice = unitPrice
                }.RecalculateAmountsAsync();
            });

            var coinItems = (await Task.WhenAll(coinItemTasks)).ToList();

            Dialog.Close(coinItems);
        }

        else if (_currencies is not null)
        {
            var currencyItemTasks = _currencies.Select(async currency =>
            {
                var unitPrice = await GetCurrencyPriceAsync(currency.Id);

                return await new CurrencyItemVm
                {
                    Currency = new GetPriceUnitTitleResponse(currency.Id, currency.Title, currency.HasIcon,
                        currency.IsDefault, false),
                    Amount = 1,
                    UnitPrice = unitPrice
                }.RecalculateAmountsAsync();
            });

            var currencyItems = (await Task.WhenAll(currencyItemTasks)).ToList();

            Dialog.Close(currencyItems);
        }

        else
            Close();
    }

    private async Task<decimal> GetCurrencyPriceAsync(Guid currencyId)
    {
        decimal currencyPrice = 0;

        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(currencyId, PriceUnit.Id, ct),
            afterSend: response =>
            {
                currencyPrice = response.ExchangeRate ?? 0;
            },
            createScope: true);

        return currencyPrice;
    }

    private async Task<decimal> GetCoinPriceAsync(Guid coinId)
    {
        decimal coinPrice = 0;

        await SendRequestAsync<ICoinService, GetExchangeRateResponse?>(
            action: (s, ct) => s.GetPriceAsync(coinId, PriceUnit.IsDefault ? null : PriceUnit.Id, ct),
            afterSend: response =>
            {
                if (response is null)
                    return;

                coinPrice = response.ExchangeRate ?? 0;
            },
            createScope: true);

        return coinPrice;
    }

    private void Close() => Dialog.Close();
}