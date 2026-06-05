using GoldEx.Shared.DTOs.CheckPayments;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.Checks.Components;

public partial class AcceptCheckDialog
{
    [Parameter, EditorRequired] public GetCheckPaymentListResponse Check { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance Dialog { get; set; } = default!;

    private MudForm _form = default!;
    private GetFinancialAccountTitleResponse? _selectedAccount;
    private List<GetFinancialAccountTitleResponse>? _financialAccounts;
    private string? _description;
    private bool _processing;

    protected override async Task OnInitializedAsync()
    {
        if (Check.InvoiceType == InvoiceType.Sell)
        {
            await LoadFinancialAccountsAsync();
        }
        await base.OnInitializedAsync();
    }

    private async Task LoadFinancialAccountsAsync()
    {
        await SendRequestAsync<IFinancialAccountService, List<GetFinancialAccountTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(null, Check.PriceUnitId, ct),
            afterSend: response => _financialAccounts = response);
    }

    private Task<IEnumerable<GetFinancialAccountTitleResponse>> SearchFinancialAccountsAsync(string value, CancellationToken token)
    {
        if (_financialAccounts == null)
            return Task.FromResult<IEnumerable<GetFinancialAccountTitleResponse>>([]);

        return Task.FromResult<IEnumerable<GetFinancialAccountTitleResponse>>(
            string.IsNullOrEmpty(value)
                ? _financialAccounts
                : _financialAccounts.Where(acc => acc.Title.Contains(value, StringComparison.OrdinalIgnoreCase))
        );
    }

    private void OnAccountChanged(GetFinancialAccountTitleResponse? account)
    {
        _selectedAccount = account;
    }

    private void Close() => Dialog.Cancel();

    private async Task Submit()
    {
        if (Check.InvoiceType == InvoiceType.Sell)
        {
            await _form.ValidateAsync();
            if (!_form.IsValid || _selectedAccount == null)
                return;
        }

        _processing = true;
        StateHasChanged();

        var request = new AcceptCheckPaymentRequest(
            _selectedAccount?.Id ?? Check.IssuerFinancialAccountId,
            _description
        );

        await SendRequestAsync<ICheckPaymentService>(
            action: (s, ct) => s.AcceptAsync(Check.Id, request, ct),
            afterSend: () => { Dialog.Close(DialogResult.Ok(true)); return Task.CompletedTask; },
            onFailure: () => {
                _processing = false;
                StateHasChanged();
                return Task.CompletedTask;
            }
        );
    }
}
