using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.FinancialAccounts.Components;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.PaymentVouchers.Components;
using GoldEx.Shared.DTOs.Customers;
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

    private List<Guid> _voucherIds = [];
    private List<GetCustomerResponse> _customers = [];

    private decimal GetTotalPaid()
    {
        return Items.Sum(x => x.FinalAmount * (x.ExchangeRate ?? 1));
    }

    private decimal TotalRemainingCalculated => TotalInvoiceAmount - GetTotalPaid();

    public string FinancialAccountLabelText => InvoiceType switch
    {
        InvoiceType.Purchase => "پرداخت از حساب",
        InvoiceType.Sell => "پرداخت به حساب",
        _ => throw new ArgumentOutOfRangeException()
    };

    protected override void OnParametersSet()
    {
        _voucherIds = Items.Where(x => x.VoucherId.HasValue)
            .Select(x => x.VoucherId!.Value)
            .ToList();

        base.OnParametersSet();
    }

    protected override async Task OnInitializedAsync()
    {
        if (!Items.Any())
            await AddItem();

        await base.OnInitializedAsync();
    }

    private async Task LoadFinancialAccountsAsync(InvoicePaymentVm item)
    {
        await SendRequestAsync<IFinancialAccountService, List<GetFinancialAccountTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(null, item.PriceUnit?.Id ?? PriceUnit.Id, ct),
            afterSend: response => item.FinancialAccounts = response);
    }

    private async Task AddItem()
    {
        var item = new InvoicePaymentVm
        {
            Amount = 0m,
            Note = string.Empty,
            PaymentDate = DateTime.Now,
            PriceUnit = PriceUnit,
            AmountAdornmentText = PriceUnit.Title
        };

        Items.Add(item);

        await LoadFinancialAccountsAsync(item);
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
        item.PriceUnit = priceUnit;
        item.AmountAdornmentText = priceUnit.Title;
        item.ExchangeRateLabel = $"نرخ تبدیل {item.PriceUnit.Title} به {PriceUnit.Title}";

        await LoadFinancialAccountsAsync(item);

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

    private async Task OnTotalRemainingClicked()
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
            var item = new InvoicePaymentVm
            {
                Amount = remaining,
                AmountAdornmentText = PriceUnit.Title,
                PriceUnit = PriceUnit,
                PaymentDate = DateTime.Now
            };

            Items.Add(item);

            await LoadFinancialAccountsAsync(item);
        }
    }

    private async Task OnAddFinancialAccount(InvoicePaymentVm item)
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
            { x => x.IsSystemAccount, true },
            { x => x.SubmitIndependently, true }
        };

        var dialog = await DialogService.ShowAsync<FinancialAccountEditor>("افزودن حساب مالی جدید",
            parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: FinancialAccountVm })
        {
            await LoadFinancialAccountsAsync(item);
            StateHasChanged();
        }
    }

    #region PaymentVoucher

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

    #endregion

    #region Customer

    private async Task<IEnumerable<CustomerVm>?> SearchCustomers(string? customerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            return null;

        await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
            action: (s, ct) => s.GetByNameAsync(customerName, null, ct),
            afterSend: response =>
            {
                _customers = response;
            },
            cancelPrevious: true);

        return _customers.Select(CustomerVm.CreateFrom);
    }

    private async Task OnAddCustomer(InvoicePaymentVm context)
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var parameters = new DialogParameters<Customers.Components.Editor>
        {
            { x => x.ReturnModel, true }
        };

        var dialog = await DialogService.ShowAsync<Customers.Components.Editor>("افزودن طرف حساب جدید", parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm customerVm })
        {
            context.Endorser = customerVm;
            StateHasChanged();
        }
    }

    #endregion

    private void OnPaymentTypeChanged(InvoicePaymentVm item, PaymentType paymentType)
    {
        item.PaymentType = paymentType;

        switch (paymentType)
        {
            case PaymentType.InternalCash:
                item.GoldFineness = null;
                item.Endorser = null;
                item.FinancialAccount = null;
                break;
            case PaymentType.UsedGoldInventory:
                item.Endorser = null;
                item.FinancialAccount = null;
                item.GoldFineness = 750m;
                break;
            case PaymentType.MoltenGoldInventory:
                item.Endorser = null;
                item.FinancialAccount = null;
                item.GoldFineness = 750m;
                break;
            case PaymentType.CustomerTransfer:
                item.Endorser = null;
                item.FinancialAccount = null;
                item.GoldFineness = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(paymentType), paymentType, null);
        }
    }
}