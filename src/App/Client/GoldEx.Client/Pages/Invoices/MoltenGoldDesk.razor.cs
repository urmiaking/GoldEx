using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Invoices.Components;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.MoltenGold;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices;

public partial class MoltenGoldDesk
{
    [Inject] private ResponsiveDialogService ResponsiveDialogs { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private static readonly (MoltenGoldTradeFilter Filter, string Title)[] Filters =
    [
        (MoltenGoldTradeFilter.Today, "امروز"),
        (MoltenGoldTradeFilter.Unsettled, "تسویه‌نشده"),
        (MoltenGoldTradeFilter.Undelivered, "ماندهٔ طلا"),
        (MoltenGoldTradeFilter.All, "همه")
    ];

    private MoltenGoldDeskSettings? _settings;
    private List<GetFinancialAccountTitleResponse> _accounts = [];
    private List<MoltenGoldTradeResponse>? _rows;
    private CustomerVm? _customer;
    private bool _loadingBalances;
    private decimal? _goldBalance;
    private string _goldUnitTitle = "گرم ۱۸ عیار";
    private decimal? _moneyBalance;
    private InvoiceType _side = InvoiceType.Purchase;
    private decimal _weight;
    private decimal _fineness = 750m;
    private decimal _rate;
    private bool _advancedOpen;
    private bool _showImmediate;
    private DateTime? _rateUpdatedAt;
    private decimal _commission;
    private PayMoltenGold? _immediatePayment;
    private MoltenGoldDeliveryDraft? _immediateDelivery;
    private Guid _tradeId = Guid.NewGuid();
    private bool _saving;
    private bool _loading;
    private bool _isInitializing = true;
    private int _skip;
    private int _balanceVersion;
    private MoltenGoldTradeFilter _filter = MoltenGoldTradeFilter.Today;
    private int _mobileTab;
    private string _searchText = "";

    private IReadOnlyList<MoltenGoldTradeResponse> FilteredRows
    {
        get
        {
            if (_rows is null) return [];
            if (string.IsNullOrWhiteSpace(_searchText)) return _rows;
            var query = _searchText.Trim();
            return _rows.Where(r =>
                (r.CustomerName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                r.Events.Any(e => e.Reference?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                r.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture).Contains(query, StringComparison.Ordinal)
            ).ToList();
        }
    }

    private bool EntryDisabled => _saving || _isInitializing || _settings is null ||
        _settings.PriceUnitId == Guid.Empty || _settings.GoldPriceUnitId == Guid.Empty;

    private decimal RawValue => _weight is > 0 and <= 1_000_000m && _rate is > 0 and <= 1_000_000_000_000m &&
        _fineness is > 0 and < 1000m ? MoltenGoldTradePricing.Value(_weight, _fineness, _rate) : 0;
    private decimal CommissionValue => MoltenGoldTradePricing.Commission(RawValue, _commission);

    private string ImmediateSummary
    {
        get
        {
            if (_immediatePayment is null && _immediateDelivery is null)
                return "وجه و تحویل طلا را می‌توانید بعداً ثبت کنید.";
            var parts = new List<string>(2);
            if (_immediatePayment is not null)
                parts.Add($"وجه: {Money(_immediatePayment.Amount)}");
            if (_immediateDelivery is not null)
                parts.Add($"طلا: {WeightText(_immediateDelivery.Weight)}");
            return string.Join(" · ", parts);
        }
    }


    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        _isInitializing = true;
        try
        {
            _settings = await SendRequestAsync<IMoltenGoldTradeService, MoltenGoldDeskSettings>(
                (s, ct) => s.GetSettingsAsync(ct));
            if (_settings is null || _settings.PriceUnitId == Guid.Empty || _settings.GoldPriceUnitId == Guid.Empty)
                return;

            if (_rate == 0) _rate = _settings.DayRate;
            _rateUpdatedAt = _settings.RateUpdatedAt;
            _commission = _side == InvoiceType.Sell ? _settings.CommissionPercent : 0m;

            _accounts = await SendRequestAsync<IFinancialAccountService, List<GetFinancialAccountTitleResponse>>(
                (s, ct) => s.GetTitlesAsync(null, _settings.PriceUnitId, ct)) ?? [];
            await LoadAsync();
        }
        finally
        {
            _isInitializing = false;
        }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            _rows = await SendRequestAsync<IMoltenGoldTradeService, List<MoltenGoldTradeResponse>>(
                (s, ct) => s.GetListAsync(_filter, _skip, ct));
        }
        finally { _loading = false; }
    }

    private async Task ChangeFilterAsync(MoltenGoldTradeFilter filter)
    {
        if (_filter == filter) return;
        _filter = filter;
        _skip = 0;
        await LoadAsync();
    }

    private async Task ChangePageAsync(int delta)
    {
        _skip = Math.Max(0, _skip + delta);
        await LoadAsync();
    }

    private void ToggleAdvanced() => _advancedOpen = !_advancedOpen;
    private void ToggleImmediate() => _showImmediate = !_showImmediate;

    private void ChangeSide(InvoiceType side)
    {
        if (_side == side) return;
        _side = side;
        _commission = side == InvoiceType.Sell ? _settings?.CommissionPercent ?? 0m : 0m;
        _immediatePayment = null;
        _immediateDelivery = null;
    }

    private async Task SelectCustomer(CustomerVm? customer)
    {
        _customer = customer;
        await LoadCustomerBalancesAsync();
    }

    private async Task<IEnumerable<CustomerVm>> SearchCustomersAsync(string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        var customers = await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
            (s, _) => s.GetByNameAsync(text.Trim(), null, ct), createScope: true);
        return customers?.Select(CustomerVm.CreateFrom) ?? [];
    }

    private async Task AddCustomerAsync()
    {
        var dialog = ResponsiveDialogs.Show<Customers.Components.Editor>("افزودن طرف‌حساب جدید", new()
        {
            [nameof(Customers.Components.Editor.ReturnModel)] = true,
            [nameof(Customers.Components.Editor.ShowFinancialAccounts)] = false
        });
        if (await dialog.Result is { Canceled: false, Data: CustomerVm customer }) await SelectCustomer(customer);
    }

    private async Task OpenImmediateAsync(MoltenGoldOperation operation)
    {
        if (_weight <= 0 || _rate <= 0 || _fineness <= 0)
        {
            Snackbar.Add("ابتدا وزن، عیار و نرخ معامله را وارد کنید.", Severity.Warning);
            return;
        }
        var args = new Dictionary<string, object>
        {
            [nameof(MoltenGoldOperationDialog.Operation)] = operation,
            [nameof(MoltenGoldOperationDialog.Side)] = _side,
            [nameof(MoltenGoldOperationDialog.LimitAmount)] = RawValue + CommissionValue,
            [nameof(MoltenGoldOperationDialog.LimitWeight)] = _weight,
            [nameof(MoltenGoldOperationDialog.Fineness)] = _fineness,
            [nameof(MoltenGoldOperationDialog.Settings)] = _settings!,
            [nameof(MoltenGoldOperationDialog.Accounts)] = _accounts
        };
        if (operation == MoltenGoldOperation.Payment && _immediatePayment is not null)
            args[nameof(MoltenGoldOperationDialog.InitialPayment)] = _immediatePayment;
        if (operation == MoltenGoldOperation.Delivery && _immediateDelivery is not null)
            args[nameof(MoltenGoldOperationDialog.InitialDelivery)] = _immediateDelivery;
        var dialog = ResponsiveDialogs.Show<MoltenGoldOperationDialog>(OperationTitle(operation, _side), args);
        var result = await dialog.Result;
        if (result.Canceled) return;
        if (result.Data is PayMoltenGold payment) _immediatePayment = payment.Amount > 0 ? payment : null;
        if (result.Data is MoltenGoldDeliveryDraft delivery) _immediateDelivery = delivery.Weight > 0 ? delivery : null;
    }

    private async Task OpenTradeAsync(MoltenGoldTradeResponse trade)
    {
        var details = ResponsiveDialogs.Show<MoltenGoldTradeDetails>("جزئیات معاملهٔ آب‌شده", new()
        {
            [nameof(MoltenGoldTradeDetails.Trade)] = trade
        }, wide: true);
        if (await details.Result is not { Canceled: false, Data: MoltenGoldOperation operation }) return;
        var dialog = ResponsiveDialogs.Show<MoltenGoldOperationDialog>(OperationTitle(operation, trade.Side), new()
        {
            [nameof(MoltenGoldOperationDialog.Operation)] = operation,
            [nameof(MoltenGoldOperationDialog.Trade)] = trade,
            [nameof(MoltenGoldOperationDialog.Settings)] = _settings!,
            [nameof(MoltenGoldOperationDialog.Accounts)] = _accounts
        });
        if (await dialog.Result is { Canceled: false, Data: true })
        {
            _balanceVersion++;
            Snackbar.Add("پیگیری معامله ثبت شد.", Severity.Success);
            await LoadAsync();
        }
    }

    private async Task CreateAsync()
    {
        if (EntryDisabled) return;
        if (_customer?.Id is not Guid customerId || _weight is <= 0 or > 1_000_000m || _rate is <= 0 or > 1_000_000_000_000m ||
            _fineness is <= 0 or >= 1000m || _immediateDelivery?.Weight > _weight ||
            _immediatePayment?.Amount > RawValue + CommissionValue)
        {
            Snackbar.Add("طرف‌حساب، وزن، عیار، نرخ و اطلاعات لازم معامله را کامل کنید.", Severity.Warning);
            return;
        }
        var request = new CreateMoltenGoldTrade(_tradeId, _side, customerId, _immediateDelivery?.Stock?.Id, _weight, _fineness,
            MoltenGoldRateBasis.Gram, _rate, _commission, _immediateDelivery?.AssayNumber, _immediateDelivery?.Weight ?? 0,
            _immediatePayment?.AccountId, _immediatePayment?.Amount ?? 0, _immediatePayment?.Reference, IsPaper: true);
        _saving = true;
        try
        {
            await SendRequestAsync<IMoltenGoldTradeService>((s, ct) => s.CreateAsync(request, ct), afterSend: async () =>
            {
                _tradeId = Guid.NewGuid();
                _weight = 0;
                _showImmediate = false;
                _commission = _side == InvoiceType.Sell ? _settings?.CommissionPercent ?? 0m : 0m;
                _immediatePayment = null;
                _immediateDelivery = null;
                _filter = MoltenGoldTradeFilter.Today;
                _skip = 0;
                _balanceVersion++;
                await LoadCustomerBalancesAsync();
                Snackbar.Add("معامله ثبت شد.", Severity.Success);
                await LoadAsync();
            }, ignoreCancellation: true);
        }
        finally { _saving = false; }
    }

    private static string OperationTitle(MoltenGoldOperation operation, InvoiceType side) => operation switch
    {
        MoltenGoldOperation.Payment => side == InvoiceType.Sell ? "دریافت وجه" : "پرداخت وجه",
        MoltenGoldOperation.Delivery => side == InvoiceType.Sell ? "تحویل طلا" : "دریافت طلا",
        MoltenGoldOperation.Transfer => "حواله بین طرف‌حساب‌ها",
        MoltenGoldOperation.GoldSettlement => "تسویهٔ طلایی",
        _ => "لغو توافق"
    };
    private static string CustomerName(CustomerVm? customer) => customer?.FullName ?? "";
    private static string Money(decimal amount) => $"{amount:#,##0.##} تومان";
    private static string WeightText(decimal amount) => $"{amount:0.###} گرم";
    private static string DateText(DateTime date)
    {
        var calendar = new System.Globalization.PersianCalendar();
        return $"{calendar.GetYear(date)}/{calendar.GetMonth(date):00}/{calendar.GetDayOfMonth(date):00} · {date:HH:mm}";
    }

    private async Task LoadCustomerBalancesAsync()
    {
        if (_customer?.Id == null)
        {
            _goldBalance = null;
            _moneyBalance = null;
            return;
        }

        _loadingBalances = true;
        try
        {
            var balances = await SendRequestAsync<ITransactionService, List<GetCustomerRemainingResponse>>(
                (s, ct) => s.GetCustomerRemainingListAsync(_customer.Id.Value, null, null, null, ct));

            if (balances is null)
            {
                _goldBalance = 0m;
                _moneyBalance = 0m;
                return;
            }

            // Select exact configured units. Never infer a second balance by partial unit titles.
            var goldRemaining = balances.FirstOrDefault(b => b.PriceUnit.Id == _settings!.GoldPriceUnitId);
            var moneyRemaining = balances.FirstOrDefault(b => b.PriceUnit.Id == _settings!.PriceUnitId);
            _goldBalance = goldRemaining?.Amount ?? 0m;
            _goldUnitTitle = goldRemaining?.PriceUnit.Title ?? "گرم ۱۸ عیار";
            _moneyBalance = moneyRemaining?.Amount ?? 0m;
        }
        catch (Exception)
        {
            _goldBalance = null;
            _moneyBalance = null;
        }
        finally
        {
            _loadingBalances = false;
            StateHasChanged();
        }
    }

    private static string GetBalanceStatusClass(decimal? balance) => balance switch
    {
        > 0 => "is-debit",
        < 0 => "is-credit",
        _ => "is-settled"
    };

    private static string GetBalanceStatusText(decimal? balance) => balance switch
    {
        > 0 => "بدهکار",
        < 0 => "بستانکار",
        _ => "تسویه"
    };

    private string FormatGoldBalance(decimal? balance)
    {
        if (!balance.HasValue) return "-";
        return $"{Math.Abs(balance.Value):0.###} گرم";
    }

    private static string FormatMoneyBalance(decimal? balance)
        => !balance.HasValue ? "-" : $"{Math.Abs(balance.Value):#,##0.##} تومان";
}
