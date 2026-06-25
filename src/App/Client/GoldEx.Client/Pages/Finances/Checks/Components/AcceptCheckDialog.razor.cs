using GoldEx.Shared.DTOs.CheckPayments;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.Checks.Components;

public partial class AcceptCheckDialog
{
    [Parameter, EditorRequired] public GetCheckPaymentListResponse Check { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance Dialog { get; set; } = default!;
    [Inject] public IPriceService PriceService { get; set; } = default!;

    private MudForm _form = default!;
    private GetFinancialAccountTitleResponse? _selectedAccount;
    private List<GetFinancialAccountTitleResponse>? _financialAccounts;
    private string? _description;
    private bool _processing;

    private decimal? _cashingExchangeRate;
    private CheckCashingAdjustmentMode? _adjustmentMode = CheckCashingAdjustmentMode.FixedRate;
    private decimal _originalValue;
    private decimal _cashingValue;
    private decimal _difference;
    private string _adjustmentExplanation = string.Empty;
    private bool _settleDifference;
    private GetFinancialAccountTitleResponse? _settlementAccount;
    private List<GetFinancialAccountTitleResponse>? _settlementFinancialAccounts;

    private bool IsMultiCurrency => Check.PriceUnitId != Check.InvoicePriceUnitId && Check.InvoicePriceUnitId != Guid.Empty;

    protected override async Task OnInitializedAsync()
    {
        if (Check.InvoiceType == InvoiceType.Sell)
        {
            await LoadFinancialAccountsAsync();
        }

        if (IsMultiCurrency)
        {
            // Default cashing rate to the check's original exchange rate in the UI format
            if (Check.OriginalExchangeRate.HasValue && Check.OriginalExchangeRate.Value > 0)
            {
                bool checkIsGold = IsGold(Check.PriceUnit);
                bool invoiceIsGold = IsGold(Check.InvoicePriceUnitTitle);

                if (checkIsGold && !invoiceIsGold)
                {
                    _cashingExchangeRate = Check.OriginalExchangeRate.Value;
                }
                else if (!checkIsGold && invoiceIsGold)
                {
                    _cashingExchangeRate = 1m / Check.OriginalExchangeRate.Value;
                }
                else
                {
                    _cashingExchangeRate = Check.OriginalExchangeRate.Value;
                }
            }
            RecalculatePreviews();
        }

        await base.OnInitializedAsync();
    }

    private bool IsGold(string title)
    {
        if (string.IsNullOrEmpty(title)) return false;
        var t = title.ToLower();
        return t.Contains("گرم") || t.Contains("مثقال") || t.Contains("gram") || t.Contains("gold");
    }

    private void OnCashingExchangeRateChanged(decimal? value)
    {
        _cashingExchangeRate = value;
        RecalculatePreviews();
    }

    private async Task OnAdjustmentModeChanged(CheckCashingAdjustmentMode? value)
    {
        _adjustmentMode = value;
        if (value == CheckCashingAdjustmentMode.DailyRate)
        {
            await LoadCurrentPriceAsync();
            if (_settlementFinancialAccounts == null)
            {
                await LoadSettlementFinancialAccountsAsync();
            }
        }
        else
        {
            _settleDifference = false;
            _settlementAccount = null;
            RecalculatePreviews();
        }
    }

    private async Task LoadCurrentPriceAsync()
    {
        _processing = true;
        StateHasChanged();
        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(Check.InvoicePriceUnitId, Check.PriceUnitId, ct),
            afterSend: response =>
            {
                _cashingExchangeRate = response.ExchangeRate;
                _processing = false;
                RecalculatePreviews();
                StateHasChanged();
                return Task.CompletedTask;
            },
            onFailure: () =>
            {
                _processing = false;
                StateHasChanged();
                return Task.CompletedTask;
            });
    }

    private void RecalculatePreviews()
    {
        if (!IsMultiCurrency) return;

        bool invoiceIsGold = IsGold(Check.InvoicePriceUnitTitle);
        bool checkIsGold = IsGold(Check.PriceUnit);

        // Original value in invoice price unit
        _originalValue = Check.Amount * (Check.OriginalExchangeRate ?? 1m);

        // Cashing value in invoice price unit
        if (_cashingExchangeRate.HasValue && _cashingExchangeRate.Value > 0)
        {
            if (checkIsGold && !invoiceIsGold)
            {
                _cashingValue = Check.Amount * _cashingExchangeRate.Value;
            }
            else if (!checkIsGold && invoiceIsGold)
            {
                _cashingValue = Check.Amount / _cashingExchangeRate.Value;
            }
            else
            {
                _cashingValue = Check.Amount * _cashingExchangeRate.Value;
            }
        }
        else
        {
            _cashingValue = 0m;
        }

        _difference = _cashingValue - _originalValue;
        UpdateExplanation(invoiceIsGold);
    }

    private void UpdateExplanation(bool invoiceIsGold)
    {
        if (_adjustmentMode == CheckCashingAdjustmentMode.DailyRate)
        {
            if (Math.Abs(_difference) < 0.0001m)
            {
                _adjustmentExplanation = "تفاوت قیمتی وجود ندارد و حسابی تعدیل نخواهد شد.";
                return;
            }

            decimal diffToman = DifferenceToman;

            if (_difference < 0)
            {
                _adjustmentExplanation = Check.InvoiceType == InvoiceType.Sell
                    ? $"حساب مشتری به میزان {Math.Abs(_difference):#,##0.###} {Check.InvoicePriceUnitTitle} (معادل {Math.Abs(diffToman):#,##0} تومان) بدهکار خواهد شد (مشتری باید این مقدار مابه‌التفاوت را پرداخت کند)."
                    : $"حساب تأمین‌کننده به میزان {Math.Abs(_difference):#,##0.###} {Check.InvoicePriceUnitTitle} (معادل {Math.Abs(diffToman):#,##0} تومان) بستانکار خواهد شد (ما این مقدار مابه‌التفاوت را به تأمین‌کننده بدهکار می‌شویم).";
            }
            else
            {
                _adjustmentExplanation = Check.InvoiceType == InvoiceType.Sell
                    ? $"حساب مشتری به میزان {Math.Abs(_difference):#,##0.###} {Check.InvoicePriceUnitTitle} (معادل {Math.Abs(diffToman):#,##0} تومان) بستانکار خواهد شد (مشتری بستانکار می‌شود)."
                    : $"حساب تأمین‌کننده به میزان {Math.Abs(_difference):#,##0.###} {Check.InvoicePriceUnitTitle} (معادل {Math.Abs(diffToman):#,##0} تومان) بدهکار خواهد شد (تأمین‌کننده به ما بدهکار می‌شود).";
            }
        }
        else
        {
            // Fixed Rate Mode (Store Absorbs Gain/Loss)
            decimal diffToman = DifferenceToman;
            
            if (Math.Abs(diffToman) < 0.01m)
            {
                _adjustmentExplanation = "تفاوت قیمتی وجود ندارد و تراکنش تسعیر ثبت نخواهد شد.";
                return;
            }

            bool isLoss = (Check.InvoiceType == InvoiceType.Sell && diffToman < 0) || (Check.InvoiceType == InvoiceType.Purchase && diffToman > 0);

            _adjustmentExplanation = isLoss
                ? $"تسویه با قیمت قطعی انجام می‌شود. مبلغ {Math.Abs(diffToman):#,##0} تومان به عنوان زیان تسعیر برای فروشگاه ثبت می‌شود و حساب مشتری تغییر نخواهد کرد."
                : $"تسویه با قیمت قطعی انجام می‌شود. مبلغ {Math.Abs(diffToman):#,##0} تومان به عنوان سود تسعیر برای فروشگاه ثبت می‌شود و حساب مشتری تغییر نخواهد کرد.";
        }
    }

    private string FormatInvoiceAmount(decimal amount) => amount.ToCurrencyFormat(Check.InvoicePriceUnitTitle);
    
    private string FormatTomanAmount(decimal amount) => amount.ToCurrencyFormat("تومان");

    private decimal DifferenceToman => _difference * (IsGold(Check.InvoicePriceUnitTitle) && _cashingExchangeRate.HasValue ? _cashingExchangeRate.Value : 1m);

    private Severity GetAlertSeverity() => Severity.Info;

    private string GetDifferenceColorClass()
    {
        if (Math.Abs(_difference) < 0.0001m) return "";
        return _difference > 0 ? "text-success" : "text-danger";
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
        await _form.ValidateAsync();
        if (!_form.IsValid)
            return;

        if (Check.InvoiceType == InvoiceType.Sell && _selectedAccount == null)
            return;

        _processing = true;
        StateHasChanged();

        var request = new AcceptCheckPaymentRequest(
            _selectedAccount?.Id ?? Check.IssuerFinancialAccountId,
            _description,
            _cashingExchangeRate,
            _adjustmentMode,
            _settleDifference,
            _settlementAccount?.Id
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

    private async Task LoadSettlementFinancialAccountsAsync()
    {
        await SendRequestAsync<IFinancialAccountService, List<GetFinancialAccountTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(null, null, ct),
            afterSend: response => _settlementFinancialAccounts = response);
    }

    private Task<IEnumerable<GetFinancialAccountTitleResponse>> SearchSettlementFinancialAccountsAsync(string value, CancellationToken token)
    {
        if (_settlementFinancialAccounts == null)
            return Task.FromResult<IEnumerable<GetFinancialAccountTitleResponse>>([]);

        return Task.FromResult<IEnumerable<GetFinancialAccountTitleResponse>>(
            string.IsNullOrEmpty(value)
                ? _settlementFinancialAccounts
                : _settlementFinancialAccounts.Where(acc => acc.Title.Contains(value, StringComparison.OrdinalIgnoreCase))
        );
    }

    private void OnSettleDifferenceChanged(bool value)
    {
        _settleDifference = value;
        if (!value)
        {
            _settlementAccount = null;
        }
    }
}
