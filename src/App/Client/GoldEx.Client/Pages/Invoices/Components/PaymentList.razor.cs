using GoldEx.Client.Pages.Customers.Components;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.PaymentVouchers.Components;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class PaymentList
{
    [Parameter] public List<InvoicePaymentVm> Items { get; set; } = [];
    [Parameter] public GetPriceUnitTitleResponse PriceUnit { get; set; } = default!;
    [Parameter] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];
    [Parameter] public decimal TotalInvoiceAmount { get; set; }
    [Parameter] public InvoiceType InvoiceType { get; set; }
    [Parameter] public Guid? CustomerId { get; set; }

    private List<GetFinancialAccountTitleResponse> _financialAccounts = [];
    private List<Guid> _voucherIds = [];

    private decimal GetTotalPaid()
    {
        return Items.Sum(x => x.Amount * (x.ExchangeRate ?? 1));
    }

    private decimal TotalRemainingCalculated => TotalInvoiceAmount - GetTotalPaid();

    public string FinancialAccountLabelText => InvoiceType switch
    {
        InvoiceType.Purchase => "پرداخت از حساب",
        InvoiceType.Sell => "پرداخت به حساب",
        _ => throw new ArgumentOutOfRangeException()
    };

    protected override async Task OnParametersSetAsync()
    {
        if (!_financialAccounts.Any()) 
            await LoadFinancialAccountsAsync();

        await base.OnParametersSetAsync();
    }

    protected override void OnParametersSet()
    {
        _voucherIds = Items.Where(x => x.VoucherId.HasValue)
            .Select(x => x.VoucherId!.Value)
            .ToList();

        base.OnParametersSet();
    }

    protected override void OnInitialized()
    {
        if (!Items.Any())
            AddItem();

        base.OnInitialized();
    }

    private async Task LoadFinancialAccountsAsync()
    {
        await SendRequestAsync<IFinancialAccountService, List<GetFinancialAccountTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(null, PriceUnit.Id, ct),
            afterSend: response =>
            {
                _financialAccounts = response;
                Items.First().FinancialAccount = _financialAccounts.FirstOrDefault();

                StateHasChanged();
            });
    }

    private void AddItem()
    {
        Items.Add(new InvoicePaymentVm
        {
            Amount = 0m,
            Note = string.Empty,
            PaymentDate = DateTime.Now,
            PriceUnit = PriceUnit,
            AmountAdornmentText = PriceUnit.Title
        });
    }

    private void RemoveItem(InvoicePaymentVm item)
    {
        switch (Items.Count)
        {
            case > 1:
                Items.Remove(item);
                break;
            case 1:
                Items.First().Amount = 0;
                Items.First().Note = null;
                Items.First().ReferenceNumber = null;
                break;
        }
    }

    private void OnAmountChanged(decimal? amount, InvoicePaymentVm item)
    {
        if (amount.HasValue)
            item.Amount = amount.Value;
    }

    private async Task SelectPriceUnit(GetPriceUnitTitleResponse priceUnit, InvoicePaymentVm item)
    {
        PriceUnit = priceUnit;

        item.PriceUnit = priceUnit;
        item.AmountAdornmentText = priceUnit.Title;
        item.ExchangeRateLabel = $"نرخ تبدیل {item.PriceUnit.Title} به {PriceUnit.Title}";

        await LoadFinancialAccountsAsync();

        if (PriceUnit.Id == priceUnit.Id)
        {
            StateHasChanged();
            return;
        }

        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(priceUnit.Id, PriceUnit.Id, ct),
            afterSend: response =>
            {
                if (response.ExchangeRate.HasValue)
                    item.ExchangeRate = response.ExchangeRate.Value;

                StateHasChanged();
            });
    }

    private void OnTotalRemainingClicked()
    {
        var remaining = TotalRemainingCalculated;
        if (remaining <= 0)
            return;

        if (Items.Count == 1 && Items.First().Amount == 0)
        {
            var item = Items.First();

            item.Amount = remaining;
            item.AmountAdornmentText = PriceUnit.Title;
            item.PriceUnit = PriceUnit;
        }
        else if (Items.Count > 1 && Items.Last().Amount == 0)
        {
            var item = Items.Last();

            item.Amount = remaining;
            item.AmountAdornmentText = PriceUnit.Title;
            item.PriceUnit = PriceUnit;
        }
        else
        {
            Items.Add(new InvoicePaymentVm
            {
                Amount = remaining,
                AmountAdornmentText = PriceUnit.Title,
                PriceUnit = PriceUnit,
                PaymentDate = DateTime.Now
            });
        }
    }

    private async Task OnAddFinancialAccount()
    {
        DialogOptions dialogOptions = new()
        {
            CloseButton = true,
            FullWidth = true,
            FullScreen = false,
            MaxWidth = MaxWidth.Small
        };

        var parameters = new DialogParameters<FinancialAccountEditor>
        {
            { x => x.PriceUnits, PriceUnits },
            { x => x.SubmitIndependently, true }
        };

        var dialog = await DialogService.ShowAsync<FinancialAccountEditor>("افزودن حساب مالی جدید",
            parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: FinancialAccountVm })
        {
            await LoadFinancialAccountsAsync();
            StateHasChanged();
        }
    }

    private async Task OnAddPaymentVoucher()
    {
        DialogOptions dialogOptions = new()
        {
            CloseButton = true,
            FullWidth = true,
            FullScreen = false,
            MaxWidth = MaxWidth.Medium
        };

        if (CustomerId is null)
        {
            AddErrorToast("لطفاً ابتدا تامین کننده را انتخاب کنید");
            return;
        }

        var parameters = new DialogParameters<PaymentVouchersSelectorList>
        {
            {x => x.CustomerId, CustomerId.Value },
            {x => x.SelectedPaymentVouchers, _voucherIds }
        };

        var dialog = await DialogService.ShowAsync<PaymentVouchersSelectorList>("اسناد پرداخت", parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: HashSet<GetPaymentVoucherResponse> paymentVouchers })
        {
            AddPaymentVouchersToList(paymentVouchers);
            StateHasChanged();
        }
    }

    private void AddPaymentVouchersToList(HashSet<GetPaymentVoucherResponse> paymentVouchers)
    {
        foreach (var paymentVoucher in paymentVouchers)
        {
            Items.Add(new InvoicePaymentVm
            {
                VoucherId = paymentVoucher.Id,
                Amount = paymentVoucher.Amount,
                Note = paymentVoucher.Description,
                ReferenceNumber = paymentVoucher.VoucherNumber.ToString(),
                PaymentDate = new DateTime(paymentVoucher.PaymentDate.Year, paymentVoucher.PaymentDate.Month, paymentVoucher.PaymentDate.Day),
                PriceUnit = paymentVoucher.PriceUnit,
                AmountAdornmentText = paymentVoucher.PriceUnit.Title,
                ExchangeRate = paymentVoucher.ExchangeRate,
                Disabled = true
            });
        }
    }
}