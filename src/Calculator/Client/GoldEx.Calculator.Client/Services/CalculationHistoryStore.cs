using Blazored.LocalStorage;
using GoldEx.Calculator.Client.ViewModels;

namespace GoldEx.Calculator.Client.Services;

public class CalculationHistoryStore(ILocalStorageService localStorage)
{
    private const string StorageKey = "GoldEx_Calc_History";
    private const int MaxItems = 15;

    public event EventHandler? Changed;

    public async Task<List<CalculationHistoryItem>> GetAllAsync()
    {
        try
        {
            if (!OperatingSystem.IsBrowser())
                return [];

            return await localStorage.GetItemAsync<List<CalculationHistoryItem>>(StorageKey) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task AddAsync(CalculationHistoryItem item)
    {
        try
        {
            var list = await GetAllAsync();
            list.Insert(0, item);

            if (list.Count > MaxItems)
            {
                list = list.Take(MaxItems).ToList();
            }

            if (OperatingSystem.IsBrowser())
            {
                await localStorage.SetItemAsync(StorageKey, list);
            }

            Changed?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            // ignore
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            if (OperatingSystem.IsBrowser())
            {
                await localStorage.RemoveItemAsync(StorageKey);
            }

            Changed?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            // ignore
        }
    }
}
