using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using MudBlazor;

namespace GoldEx.Client.Pages.Dashboard.Components;

public partial class AccountBalance
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

    private string GetBalance(GetAccountBalanceResponse context)
    {
        var net = Math.Abs(CalculateNet(context.Debit, context.Credit));

        return net.ToCurrencyFormat(context.PriceUnit);
    }

    private decimal CalculateNet(decimal debit, decimal credit)
    {
        return credit - debit;
    }

    private string GetBalanceIcon(GetAccountBalanceResponse context)
    {
        var net = CalculateNet(context.Debit, context.Credit);
        return net >= 0 ? Icons.Material.Outlined.ArrowDropUp : Icons.Material.Outlined.ArrowDropDown;
    }

    private Color GetBalanceColor(GetAccountBalanceResponse context)
    {
        var net = CalculateNet(context.Debit, context.Credit);
        return net >= 0 ? Color.Success : Color.Error;
    }
}