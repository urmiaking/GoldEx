using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.Prices;

public partial class Editor
{
    [Parameter, EditorRequired] public PriceSettingDto Price { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private PriceSettingVm _model = new();

    private ValidatePriceProviderResponse? _lastValidation;
    private List<PriceProviderSymbolDto> _catalog = [];
    private bool _loadingCatalogs;

    protected override async Task OnInitializedAsync()
    {
        _model = PriceSettingVm.CreateFrom(Price);

        await LoadCatalogAsync();
    }

    private async Task LoadCatalogAsync()
    {
        if (_model.ProviderType == PriceProviderType.Manual)
        {
            _catalog.Clear();
            StateHasChanged();
            return;
        }

        _loadingCatalogs = true;

        await SendRequestAsync<IPriceService, GetPriceProviderCatalogResponse>(
            action: (svc, ct) => svc.GetProviderCatalogAsync(_model.ProviderType, Price.MarketType, ct),
            afterSend: res =>
            {
                _catalog = res.Items;
                if (string.IsNullOrWhiteSpace(_model.ProviderSymbol))
                {
                    var match = _catalog.FirstOrDefault(i =>
                        string.Equals(i.Symbol, Price.Title, StringComparison.OrdinalIgnoreCase));
                    if (match is not null)
                        _model.ProviderSymbol = match.Symbol;
                }
                return Task.CompletedTask;
            });

        _loadingCatalogs = false;
    }

    private Task<IEnumerable<string>> SearchSymbols(string value, CancellationToken cancellationToken)
    {
        if (_catalog.Count == 0)
            return Task.FromResult(Enumerable.Empty<string>());

        IEnumerable<string> baseList = _catalog.Select(c => c.Symbol);

        if (!string.IsNullOrWhiteSpace(value))
        {
            var q = value.Trim();
            baseList = baseList.Where(s => s.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult(baseList.Distinct().Take(30));
    }

    private async Task ValidateAsync()
    {
        if (_model.ProviderType == PriceProviderType.Manual)
        {
            _lastValidation = new ValidatePriceProviderResponse(true, true, "حالت دستی انتخاب شده است.", Price.MarketType, null);
            return;
        }

        if (string.IsNullOrWhiteSpace(_model.ProviderSymbol))
        {
            _lastValidation = new ValidatePriceProviderResponse(false, false, "نماد را وارد کنید.", null, null);
            return;
        }

        await SendRequestAsync<IPriceService, ValidatePriceProviderResponse>(
            action: (svc, ct) => svc.ValidateProviderSymbolAsync(Price.Id, _model.ProviderType, _model.ProviderSymbol!, ct),
            afterSend: res =>
            {
                _lastValidation = res;
                return Task.CompletedTask;
            });
    }

    private async Task OnProviderTypeChanged(PriceProviderType pt)
    {
        _model.ProviderType = pt;
        _model.ProviderSymbol = null;
        _lastValidation = null;
        await LoadCatalogAsync();
    }

    private Task OnSymbolChanged(string? s)
    {
        _model.ProviderSymbol = s;
        _lastValidation = null;
        return Task.CompletedTask;
    }

    private async Task Submit()
    {
        if (IsBusy)
            return;

        if (_model.ProviderType != PriceProviderType.Manual)
        {
            if (_lastValidation?.IsSupported != true)
            {
                await ValidateAsync();
                if (_lastValidation?.IsSupported != true)
                {
                    AddErrorToast(_lastValidation?.Message ?? "اعتبارسنجی ناموفق بود.");
                    return;
                }
            }
        }

        byte[] iconBytes;
        if (_model.IconFile is not null)
        {
            await using var stream = _model.IconFile.OpenReadStream(maxAllowedSize: 2 * 1024 * 1024);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            iconBytes = ms.ToArray();
        }
        else
        {
            // If no icon selected we still must send something (or skip icon part).
            // Decide: send empty array to skip replacement.
            iconBytes = [];
        }

        var enabled = _model.ProviderType != PriceProviderType.Manual;
        var request = PriceSettingVm.ToRequest(iconBytes, _model.ProviderType, _model.ProviderSymbol, enabled);

        await SendRequestAsync<IPriceService>(
            action: (svc, ct) => svc.UpdateAsync(Price.Id, request, ct),
            afterSend: () =>
            {
                MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }

    private void Close() => MudDialog.Cancel();

    
}