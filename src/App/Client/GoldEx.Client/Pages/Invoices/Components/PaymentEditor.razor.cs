using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.FinancialAccounts.Components;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class PaymentEditor
{
    [Parameter, EditorRequired] public InvoicePaymentVm Model { get; set; }
    [Parameter, EditorRequired] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; }
    [Parameter, EditorRequired] public decimal TotalRemaining { get; set; }
    [Parameter, EditorRequired] public GetPriceUnitTitleResponse BasePriceUnit { get; set; }
    [Parameter, EditorRequired] public InvoiceType InvoiceType { get; set; }
    [CascadingParameter] private IMudDialogInstance Dialog { get; set; } = default!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;

    private List<GetCustomerResponse>? _customers;
    private List<GetTinyInvoiceResponse>? _invoices;
    private List<FinancialAccountVm> _customerFinancialAccounts = [];
    private MudForm _form = default!;
    private string? _checkImagePreviewUrl;
    private readonly InvoicePaymentValidator _paymentValidator = new();
    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        FullWidth = true,
        FullScreen = false,
        MaxWidth = MaxWidth.Small
    };

    private decimal TotalRemainingCalculated =>
        TotalRemaining - GetBalanceEffect(InvoiceType, Model);

    private const decimal Epsilon = 0.0001m;

    private bool IsZeroRemaining => Math.Abs(TotalRemainingCalculated) < Epsilon;

    private Color RemainingColor =>
        IsZeroRemaining
            ? Color.Info
            : InvoiceType switch
            {
                InvoiceType.Sell => TotalRemainingCalculated < 0
                    ? Color.Success   // بستانکاری مشتری (ما به او بدهکاریم)
                    : Color.Error,    // بدهکاری مشتری (او به ما بدهکار است)

                InvoiceType.Purchase => TotalRemainingCalculated > 0
                    ? Color.Success   // بستانکاری فروشنده (ما به او بدهکاریم)
                    : Color.Error,    // بدهکاری فروشنده (او به ما بدهکار است)

                _ => Color.Info
            };

    private string RemainingLabel =>
        IsZeroRemaining
            ? "فاکتور تسویه شده"
            : InvoiceType switch
            {
                InvoiceType.Sell => TotalRemainingCalculated > 0
                    ? "بدهی مشتری:"
                    : "بستانکاری مشتری:",

                InvoiceType.Purchase => TotalRemainingCalculated > 0
                    ? "بدهی ما به فروشنده:"
                    : "بستانکاری ما از فروشنده:",

                _ => "مانده:"
            };

    private string FinancialAccountLabelText =>
        Model.PaymentSide switch
        {
            PaymentSide.Receive => "واریز به حساب",
            PaymentSide.Pay => "پرداخت از حساب",
            _ => throw new ArgumentOutOfRangeException()
        };

    protected override async Task OnInitializedAsync()
    {
        // If we have CheckImage bytes but no preview URL, recreate the blob
        if (Model.CheckImage is { Length: > 0 } && string.IsNullOrEmpty(_checkImagePreviewUrl))
        {
            var base64 = Convert.ToBase64String(Model.CheckImage);
            _checkImagePreviewUrl = $"data:{Model.CheckImageContentType ?? "image/jpeg"};base64,{base64}";
        }
        else if (!string.IsNullOrEmpty(Model.CheckImageUrl))
        {
            _checkImagePreviewUrl = Model.CheckImageUrl + $"?v={Random.Shared.Next()}";
        }

        if (Model.PaymentType is PaymentType.InternalCash && Model.FinancialAccounts == null)
            await LoadFinancialAccountsAsync();

        if (Model.PaymentType is PaymentType.MoltenGoldInventory or PaymentType.UsedGoldInventory && Model.PriceUnit?.Id != BasePriceUnit.Id && !Model.Id.HasValue)
            await LoadExchangeRateAsync();

        await base.OnInitializedAsync();
    }

    private static decimal GetBalanceEffect(InvoiceType invoiceType, InvoicePaymentVm model)
    {
        var baseAmount = model.FinalAmount * (model.ExchangeRate ?? 1);

        if (invoiceType == InvoiceType.Purchase)
        {
            return model.PaymentSide == PaymentSide.Pay ? baseAmount : -baseAmount;
        }

        return model.PaymentSide == PaymentSide.Receive ? baseAmount : -baseAmount;
    }

    private void Close() => Dialog.Cancel();

    private async Task Submit()
    {
        await _form.ValidateAsync();

        if (!_form.IsValid)
            return;

        if (Model.CheckImageFile != null)
        {
            var imageBytes = await ReadFileBytes(Model.CheckImageFile);
            Model.CheckImageContentType = Path.GetExtension(Model.CheckImageFile.Name).TrimStart('.').ToLower(); // "jpg", "png", etc.
            Model.CheckImage = imageBytes;
            Model.CheckImageUrl = _checkImagePreviewUrl;
            Model.CheckImageFile = null;
        }

        Dialog.Close(DialogResult.Ok(Model));
    }


    #region Amounts

    private async Task SelectPriceUnit(GetPriceUnitTitleResponse priceUnit)
    {
        Model.PriceUnit = priceUnit;
        Model.AmountAdornmentText = priceUnit.Title;

        await LoadExchangeRateAsync();

        if (Model.PaymentType is PaymentType.InternalCash)
            await LoadFinancialAccountsAsync();

        if (Model.TargetInvoice != null)
            Model.TargetInvoice = null;
    }

    private void ApplyTotalRemaining()
    {
        var remaining = TotalRemainingCalculated;

        if (Math.Abs(remaining) < Epsilon)
            return;

        var baseEffectNeeded = TotalRemaining;
        var exchangeRate = Model.ExchangeRate ?? 1m;

        var amount = Math.Abs(baseEffectNeeded / exchangeRate);
        if (amount <= 0)
            return;

        Model.Amount = amount;
        Model.AmountAdornmentText = BasePriceUnit.Title;
        Model.PriceUnit = BasePriceUnit;
    }

    private async Task LoadExchangeRateAsync()
    {
        if (Model.PriceUnit is null)
            return;

        if (Model.PriceUnit.Id == BasePriceUnit.Id)
        {
            Model.ExchangeRate = 1;
            StateHasChanged();
            return;
        }

        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(
                Model.PriceUnit.Id,
                BasePriceUnit.Id,
                ct),
            afterSend: response =>
            {
                Model.ExchangeRate = response.ExchangeRate;
                StateHasChanged();
            });
    }

    #endregion

    #region FinancialAccounts

    private async Task<IEnumerable<GetFinancialAccountTitleResponse?>> SearchFinancialAccountsAsync(string value, CancellationToken token)
    {
        // Ensure accounts are loaded  
        if (Model.FinancialAccounts == null)
        {
            await LoadFinancialAccountsAsync();
        }

        // Return all accounts if search is empty, otherwise filter  
        return (string.IsNullOrEmpty(value)
            ? Model.FinancialAccounts
            : Model.FinancialAccounts?.Where(acc => acc.Title.Contains(value,
                StringComparison.InvariantCultureIgnoreCase))) ?? [];
    }

    private async Task LoadFinancialAccountsAsync()
    {
        await SendRequestAsync<IFinancialAccountService, List<GetFinancialAccountTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(null, Model.PriceUnit?.Id ?? BasePriceUnit.Id, ct),
            afterSend: response => Model.FinancialAccounts = response);
    }

    private async Task OnAddFinancialAccount()
    {
        var parameters = new DialogParameters<FinancialAccountEditor>
        {
            { x => x.PriceUnits, PriceUnits },
            { x => x.IsSystemAccount, true },
            { x => x.SubmitIndependently, true }
        };

        var dialog = await DialogService.ShowAsync<FinancialAccountEditor>("افزودن حساب مالی جدید",
            parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: FinancialAccountVm financialAccount })
        {
            await LoadFinancialAccountsAsync();
            Model.FinancialAccount = Model.FinancialAccounts?.FirstOrDefault(x => x.Id == financialAccount.Id);
            StateHasChanged();
        }
    }

    private async Task OnAddCustomerFinancialAccount(CustomerVm customer)
    {
        var parameters = new DialogParameters<FinancialAccountEditor>
        {
            { x => x.PriceUnits, PriceUnits },
            { x => x.IsSystemAccount, false },
            { x => x.CustomerId, customer.Id },
            { x => x.SubmitIndependently, true },
            { x => x.AccountHolderName, customer.FullName }
        };

        var dialog = await DialogService.ShowAsync<FinancialAccountEditor>($"افزودن حساب مالی برای {customer.FullName}",
            parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: FinancialAccountVm financialAccount })
        {
            await LoadFinancialAccountsAsync(customer.Id!.Value);
            Model.CheckIssuerFinancialAccount = financialAccount;
            StateHasChanged();
        }
    }

    private async Task LoadFinancialAccountsAsync(Guid customerId)
    {
        await SendRequestAsync<IFinancialAccountService, List<GetFinancialAccountResponse>>(
            action: (s, ct) => s.GetCustomerAccountsAsync(customerId, Model.PriceUnit?.Id, ct),
            afterSend: response => _customerFinancialAccounts = response.Select(FinancialAccountVm.CreateFrom).ToList());
    }

    #endregion

    #region Customer

    private async Task<IEnumerable<CustomerVm>?> SearchCustomers(string? customerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            return null;

        await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
            action: (s, ct) => s.GetByNameAsync(customerName, null, ct),
            afterSend: response => _customers = response,
            cancelPrevious: true);

        return _customers?.Select(CustomerVm.CreateFrom);
    }

    private async Task OnAddCustomer(string title)
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var parameters = new DialogParameters<Customers.Components.Editor>
        {
            { x => x.ReturnModel, true }
        };

        var dialog = await DialogService.ShowAsync<Customers.Components.Editor>($"افزودن {title} جدید", parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm customerVm })
        {
            Model.Endorser = customerVm;
            Model.CheckIssuer = customerVm;
            StateHasChanged();
        }
    }

    private async Task<IEnumerable<GetTinyInvoiceResponse>?> SearchCustomerInvoicesAsync(string? value, CancellationToken cancellationToken = default)
    {
        if (_invoices is null)
            await LoadCustomerInvoicesAsync();

        if (string.IsNullOrEmpty(value))
            return _invoices;

        return _invoices?.Where(inv => inv.InvoiceNumber.ToString().StartsWith(value));
    }

    private async Task LoadCustomerInvoicesAsync()
    {
        var request = new RequestFilter();

        if (Model.Endorser?.Id is null)
            return;

        await SendRequestAsync<IInvoiceService, List<GetTinyInvoiceResponse>>(
            action: (s, ct) => s.GetCustomerInvoicesAsync(Model.Endorser.Id.Value, request, ct),
            afterSend: response => _invoices = response,
            cancelPrevious: true);
    }

    private void OnEndorserChanged(CustomerVm endorser)
    {
        Model.Endorser = endorser;
        Model.TargetInvoice = null;

        _invoices = null;
    }

    private async Task OnCheckIssuerChanged(CustomerVm? issuer)
    {
        Model.CheckIssuer = issuer;

        if (issuer?.Id is null)
        {
            Model.CheckIssuerFinancialAccount = null;
            return;
        }

        await LoadFinancialAccountsAsync(issuer.Id.Value);

        if (_customerFinancialAccounts.Any()) 
            Model.CheckIssuerFinancialAccount = _customerFinancialAccounts.First();

        StateHasChanged();
    }

    #endregion

    private string? InvoiceToStringFunc(GetTinyInvoiceResponse? inv)
    {
        return inv is not null
            ? $"فاکتور {inv.InvoiceType.GetDisplayName()} " +
              $"شماره {inv.InvoiceNumber.ToString()} " +
              $"- مانده : {inv.Remaining.ToCurrencyFormat(inv.PriceUnit.Title)}"
            : null;
    }

    private async Task OnCheckImageChanged(InputFileChangeEventArgs args)
    {
        var file = args.File;

        // Revoke previous object URL if it was a blob (not a server URL)
        if (_checkImagePreviewUrl != null && _checkImagePreviewUrl.StartsWith("blob:"))
            await JsRuntime.InvokeVoidAsync("URL.revokeObjectURL", _checkImagePreviewUrl);

        // Generate local preview URL
        var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024); // 5MB
        var dotnetStream = new DotNetStreamReference(stream);
        _checkImagePreviewUrl = await JsRuntime.InvokeAsync<string>("createObjectURL", dotnetStream);
    }

    private async Task ClearCheckImage()
    {
        if (_checkImagePreviewUrl?.StartsWith("blob:") == true)
            await JsRuntime.InvokeVoidAsync("URL.revokeObjectURL", _checkImagePreviewUrl);

        _checkImagePreviewUrl = null;
        Model.CheckImageFile = null;
        Model.RemoveExistingImage = true;
    }

    private async Task<byte[]?> ReadFileBytes(IBrowserFile? file)
    {
        if (file is null) return null;

        await using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }

    public override async ValueTask DisposeAsync()
    {
        if (_checkImagePreviewUrl?.StartsWith("blob:") == true)
            await JsRuntime.InvokeVoidAsync("URL.revokeObjectURL", _checkImagePreviewUrl);

        await base.DisposeAsync();
    }
}