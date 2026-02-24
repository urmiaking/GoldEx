using Blazored.LocalStorage;
using GoldEx.Client.Components.Constants;
using GoldEx.Client.Components.Calculator.ViewModels;

namespace GoldEx.Calculator.Client.Services;

public sealed class QuickInvoiceBasketStore(ILocalStorageService localStorage)
{
    private const int MaxItems = 50;

    public event EventHandler? Changed;

    public async Task<List<QuickInvoicePayload>> GetItemsAsync()
    {
        try
        {
            return await localStorage.GetItemAsync<List<QuickInvoicePayload>>(LocalStorageKeys.QuickInvoiceBasket)
                   ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<int> GetCountAsync()
        => (await GetItemsAsync()).Count;

    public async Task AddAsync(QuickInvoicePayload item)
    {
        var items = await GetItemsAsync();

        if (items.Count >= MaxItems)
            items.RemoveAt(0);

        items.Add(item);

        try
        {
            await localStorage.SetItemAsync(LocalStorageKeys.QuickInvoiceBasket, items);
        }
        catch
        {
            // ignore storage failures
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task RemoveAtAsync(int index)
    {
        var items = await GetItemsAsync();

        if (index < 0 || index >= items.Count)
            return;

        items.RemoveAt(index);

        try
        {
            await localStorage.SetItemAsync(LocalStorageKeys.QuickInvoiceBasket, items);
        }
        catch
        {
            // ignore
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task ClearAsync()
    {
        try
        {
            await localStorage.RemoveItemAsync(LocalStorageKeys.QuickInvoiceBasket);
        }
        catch
        {
            // ignore
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }
}
