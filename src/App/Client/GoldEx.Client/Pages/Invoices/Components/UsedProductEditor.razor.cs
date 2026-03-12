using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class UsedProductEditor
{
    [Parameter] public UsedProductVm Model { get; set; } = new();
    [Parameter] public GetPriceUnitTitleResponse PriceUnit { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private readonly UsedProductValidator _usedProductValidator = new();

    private MudForm _form = default!;

    private GetSettingResponse? _settings;
    private bool _isProcessing;

    protected override async Task OnParametersSetAsync()
    {
        await LoadSettingsAsync();

        await base.OnParametersSetAsync();
    }

    private async Task LoadSettingsAsync()
    {
        await SendRequestAsync<ISettingService, GetSettingResponse?>(
            action: (s, ct) => s.GetAsync(ct),
            afterSend: response =>
            {
                _settings = response;
                Model.FinenessDeductionRate = _settings?.UsedGoldFinenessDeductionRate ?? 15;
            });
    }

    private async Task OnUnitTypeSelected(GoldUnitType unitType)
    {
        Model.UnitType = unitType;
        await LoadGramPriceAsync();
    }

    private async Task LoadGramPriceAsync()
    {
        await SendRequestAsync<IPriceService, GetPriceResponse?>(
            action: (s, ct) => s.GetAsync(Model.UnitType, PriceUnit.Id, true, ct),
            afterSend: response =>
            {
                decimal.TryParse(response?.Value, out var gramPriceValue);

                Model.GramPrice = gramPriceValue;

                StateHasChanged();
            });
    }

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

    private void Close() => MudDialog.Cancel();
}