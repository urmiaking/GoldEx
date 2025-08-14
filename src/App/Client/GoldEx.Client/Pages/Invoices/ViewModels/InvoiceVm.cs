using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using ValidationException = FluentValidation.ValidationException;

namespace GoldEx.Client.Pages.Invoices.ViewModels;

public class InvoiceVm
{
    public Guid? InvoiceId { get; set; }

    [Display(Name = "شماره فاکتور")]
    public long InvoiceNumber { get; set; }

    [Display(Name = "تاریخ سررسید")]
    public DateTime? DueDate { get; set; }

    [Display(Name = "تاریخ فاکتور")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public DateTime? InvoiceDate { get; set; }

    [Display(Name = "نوع فاکتور")]
    public InvoiceType InvoiceType { get; set; }

    public CustomerVm Customer { get; set; } = new();

    [Display(Name = "واحد ارزی فاکتور")]
    public GetPriceUnitTitleResponse? InvoicePriceUnit { get; set; }

    [Display(Name = "نرخ تبدیل")]
    public decimal? ExchangeRate { get; set; }

    public GetPriceUnitTitleResponse? UnpaidPriceUnit { get; set; }
    public decimal? UnpaidExchangeRate { get; set; }
    public decimal TotalUnpaidSecondaryAmount => TotalUnpaidAmount * UnpaidExchangeRate ?? 1;

    public List<ProductItemVm> ProductItems { get; set; } = [];
    public List<CoinItemVm> CoinItems { get; set; } = [];
    public List<CurrencyItemVm> CurrencyItems { get; set; } = [];

    public List<InvoiceDiscountVm> InvoiceDiscounts { get; set; } = [];
    public List<InvoiceExtraCostVm> InvoiceExtraCosts { get; set; } = [];
    public List<InvoicePaymentVm> InvoicePayments { get; set; } = [];

    // --- Calculated properties ---
    public decimal TotalItemsAmount => 
        ProductItems.Sum(i => i.TotalAmount) +
        CoinItems.Sum(i => i.TotalAmount) +
        CurrencyItems.Sum(i => i.TotalAmount);
    public decimal TotalItemsRawAmount => ProductItems.Sum(i => i.RawAmount) +
                                          CoinItems.Sum(i => i.RawAmount) +
                                          CurrencyItems.Sum(i => i.RawAmount);
    public decimal TotalItemsWageAmount => ProductItems.Sum(i => i.WageAmount);
    public decimal TotalItemsProfitAmount => ProductItems.Sum(i => i.ProfitAmount) +
                                             CoinItems.Sum(i => i.ProfitAmount) +
                                             CurrencyItems.Sum(i => i.ProfitAmount);
    public decimal TotalItemsTaxAmount => ProductItems.Sum(i => i.TaxAmount) +
                                          CurrencyItems.Sum(i => i.TaxAmount);
    public decimal TotalDiscountsAmount => InvoiceDiscounts.Sum(p => p.Amount * (p.ExchangeRate ?? 1));
    public decimal TotalExtraCostsAmount => InvoiceExtraCosts.Sum(p => p.Amount * (p.ExchangeRate ?? 1));
    public decimal TotalPaymentsAmount => InvoicePayments.Sum(p => p.Amount * (p.ExchangeRate ?? 1));
    public decimal TotalInvoiceAmount => TotalItemsAmount - TotalDiscountsAmount + TotalExtraCostsAmount;
    public decimal TotalUnpaidAmount => TotalInvoiceAmount - TotalPaymentsAmount;
    public bool IsPaid => TotalUnpaidAmount <= 0;
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now && !IsPaid;

    public static InvoiceVm CreateDefaultInstance()
    {
        return new InvoiceVm
        {
            InvoiceDate = DateTime.Now,
            Customer = new CustomerVm()
        };
    }

    public int GetLastProductIndexNumber()
    {
        return ProductItems.Count > 0 ? ProductItems.Max(i => i.Index) : 0;
    }

    public int GetLastCoinIndexNumber()
    {
        return CoinItems.Count > 0 ? CoinItems.Max(i => i.Index) : 0;
    }

    public int GetLastCurrencyIndexNumber()
    {
        return CurrencyItems.Count > 0 ? CurrencyItems.Max(i => i.Index) : 0;
    }

    public void AddProductItem(ProductItemVm productItem)
    {
        if (!ProductItems.Contains(productItem))
        {
            productItem.Index = GetLastProductIndexNumber() + 1;
            ProductItems.Add(productItem);
        }
    }

    public void AddCoinItem(CoinItemVm coinItem)
    {
        if (!CoinItems.Contains(coinItem))
        {
            coinItem.Index = GetLastCoinIndexNumber() + 1;
            CoinItems.Add(coinItem);
        }
    }

    public void AddCurrencyItem(CurrencyItemVm currencyItem)
    {
        if (!CurrencyItems.Contains(currencyItem))
        {
            currencyItem.Index = GetLastCurrencyIndexNumber() + 1;
            CurrencyItems.Add(currencyItem);
        }
    }

    /// <summary>
    /// Removes a specific item from the invoice and re-calculates the indexes of remaining items.
    /// </summary>
    /// <param name="productItemToRemove">The InvoiceItemVm instance to remove.</param>
    public void RemoveProductItem(ProductItemVm productItemToRemove)
    {
        if (ProductItems.Contains(productItemToRemove))
        {
            ProductItems.Remove(productItemToRemove);
            ReorderItemIndexes();
        }
    }

    public void RemoveCoinItem(CoinItemVm coinItem)
    {
        if (CoinItems.Contains(coinItem))
        {
            CoinItems.Remove(coinItem);
            ReorderItemIndexes();
        }
    }

    public void RemoveCurrencyItem(CurrencyItemVm currencyItem)
    {
        if (CurrencyItems.Contains(currencyItem))
        {
            CurrencyItems.Remove(currencyItem);
            ReorderItemIndexes();
        }
    }

    /// <summary>
    /// Helper method to ensure all item indexes are sequential (1, 2, 3, ...).
    /// </summary>
    private void ReorderItemIndexes()
    {
        for (var i = 0; i < ProductItems.Count; i++)
        {
            ProductItems[i].Index = i + 1;
        }

        for (var i = 0; i < CoinItems.Count; i++)
        {
            CoinItems[i].Index = i + 1;
        }

        for (var i = 0; i < CurrencyItems.Count; i++)
        {
            CurrencyItems[i].Index = i + 1;
        }
    }

    public static InvoiceRequestDto ToRequest(InvoiceVm model)
    {
        if (!model.InvoiceDate.HasValue)
            throw new ValidationException("لطفا تاریخ فاکتور را وارد کنید");

        if (model.InvoicePriceUnit == null)
            throw new ValidationException("لطفا واحد ارزی فاکتور را وارد کنید");

        if (!model.ProductItems.Any() || !model.CoinItems.Any() || !model.CurrencyItems.Any())
            throw new ValidationException("فاکتور باید حداقل دارای یک آیتم باشد");

        return new InvoiceRequestDto(model.InvoiceId,
            model.InvoiceNumber,
            model.InvoiceDate.Value,
            model.DueDate,
            model.InvoiceType,
            model.InvoicePriceUnit.Id,
            model.UnpaidExchangeRate,
            model.UnpaidPriceUnit?.Id,
            model.ExchangeRate,
            CustomerVm.ToRequest(model.Customer),
            model.ProductItems.Select(ProductItemVm.ToRequest).ToList(),
            model.CoinItems.Select(CoinItemVm.ToRequest).ToList(),
            model.CurrencyItems.Select(CurrencyItemVm.ToRequest).ToList(),
            model.InvoiceDiscounts.Select(InvoiceDiscountVm.ToRequest).ToList(),
            model.InvoicePayments.Where(x => x.Amount > 0).Select(InvoicePaymentVm.ToRequest).ToList(),
            model.InvoiceExtraCosts.Select(InvoiceExtraCostVm.ToRequest).ToList());
    }

    public static InvoiceVm CreateFrom(GetInvoiceResponse response)
    {
        return new InvoiceVm
        {
            InvoiceId = response.Id,
            InvoiceNumber = response.InvoiceNumber,
            InvoiceDate = response.InvoiceDate,
            DueDate = response.DueDate,
            Customer = CustomerVm.CreateFrom(response.Customer),
            InvoiceDiscounts = response.InvoiceDiscounts.Select(x => 
                InvoiceDiscountVm.CreateFrom(x, response.PriceUnit)).ToList(),
            InvoiceExtraCosts = response.InvoiceExtraCosts.Select(x =>
                InvoiceExtraCostVm.CreateFrom(x, response.PriceUnit)).ToList(),
            InvoicePayments = response.InvoicePayments.Select(x => 
                InvoicePaymentVm.CreateFrom(x, response.PriceUnit)).ToList(),
            ProductItems = response.InvoiceProductItems.Select(ProductItemVm.CreateFrom).ToList(),
            CoinItems = response.InvoiceCoinItems.Select(CoinItemVm.CreateFrom).ToList(),
            CurrencyItems = response.InvoiceCurrencyItems.Select(CurrencyItemVm.CreateFrom).ToList(),
            InvoicePriceUnit = response.PriceUnit,
            UnpaidExchangeRate = response.UnpaidAmountExchangeRate,
            UnpaidPriceUnit = response.UnpaidPriceUnit,
            ExchangeRate = response.ExchangeRate
        };
    }
}