using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Client.Pages.Dashboard.Components;

public partial class AccountBalanceChart
{
    private List<GetAccountBalanceResponse>? _accountBalances;

    protected override async Task OnInitializedAsync()
    {
        await LoadBalancesAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadBalancesAsync()
    {
        await SendRequestAsync<ITransactionService, List<GetAccountBalanceResponse>>(
            action: (s, ct) => s.GetAccountBalanceAsync(ct),
            afterSend: response =>
            {
                _accountBalances = response;
            });
    }
}