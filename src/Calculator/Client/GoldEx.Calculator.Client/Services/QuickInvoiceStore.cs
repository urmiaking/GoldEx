using Blazored.LocalStorage;
using GoldEx.Calculator.Client.ViewModels;
using GoldEx.Client.Components.Constants;

namespace GoldEx.Calculator.Client.Services;

public class QuickInvoiceStore(ILocalStorageService localStorage)
{
    private const string StorageKey = LocalStorageKeys.QuickInvoiceList;

    public event EventHandler? Changed;

    public async Task<List<QuickInvoice>> GetAllAsync()
    {
        try
        {
            return await localStorage.GetItemAsync<List<QuickInvoice>>(StorageKey)
                   ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task AddInvoiceAsync(QuickInvoice invoice)
    {
        var list = await GetAllAsync();
        list.Add(invoice);

        try
        {
            await localStorage.SetItemAsync(StorageKey, list);
        }
        catch
        {
            // ignore
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task RemoveAsync(string invoiceNumber)
    {
        var list = await GetAllAsync();
        list = list.Where(x => x.InvoiceNumber != invoiceNumber).ToList();

        try
        {
            await localStorage.SetItemAsync(StorageKey, list);
        }
        catch
        {
            // ignore
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }
}