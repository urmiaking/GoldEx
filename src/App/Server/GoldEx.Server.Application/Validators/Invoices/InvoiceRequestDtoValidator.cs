using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Validators.Customers;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.Invoices;

[ScopedService]
internal class InvoiceRequestDtoValidator : AbstractValidator<InvoiceRequestDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPriceUnitRepository _priceUnitRepository;
    private readonly IInventoryStockRepository _inventoryStockRepository;

    public InvoiceRequestDtoValidator(CustomerRequestDtoValidator customerValidator,
        IPriceUnitRepository priceUnitRepository, IProductRepository productRepository, IProductCategoryRepository productCategoryRepository,
        IFinancialAccountRepository financialAccountRepository, IInvoiceRepository invoiceRepository, IPaymentVoucherRepository paymentVoucherRepository,
        ICoinRepository coinRepository, IInventoryStockRepository inventoryStockRepository)
    {
        _priceUnitRepository = priceUnitRepository;
        _invoiceRepository = invoiceRepository;
        _inventoryStockRepository = inventoryStockRepository;

        RuleFor(x => x.InvoiceNumber)
            .GreaterThan(0)
            .WithMessage("لطفا شماره فاکتور را وارد کنید")
            .MustAsync(BeUniqueNumber)
            .WithMessage("شماره فاکتور نمی تواند تکراری باشد");

        RuleFor(x => x.InvoiceDate)
            .NotEmpty().WithMessage("تاریخ فاکتور الزامی است");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.InvoiceDate)
            .When(x => x.DueDate.HasValue)
            .WithMessage("تاریخ سررسید نمی‌تواند قبل از تاریخ فاکتور باشد");

        RuleFor(x => x.InvoiceType)
            .IsInEnum().WithMessage("نوع فاکتور معتبر نمی باشد");

        RuleFor(x => x.CustomerId)
            .NotNull().WithMessage("طرف حساب انتخاب نشده است");

        RuleFor(x => x)
            .Must(invoice =>
                invoice.InvoiceProductItems.Any() ||
                invoice.InvoiceCurrencyItems.Any() ||
                invoice.InvoiceCoinItems.Any() ||
                invoice.InvoiceUsedProducts.Any())
            .WithMessage("فاکتور باید حداقل دارای یک آیتم (کالا، ارز، سکه یا جنس دست دوم) باشد.");

        RuleFor(x => x)
            .Must(invoice =>
                invoice.InvoiceProductItems.Any() ||
                invoice.InvoiceCurrencyItems.Any() ||
                invoice.InvoiceCoinItems.Any())
            .When(invoice =>
                invoice.InvoiceType == InvoiceType.Sell &&
                invoice.InvoiceUsedProducts.Any())
            .WithMessage("در فاکتور فروش، کالای دست دوم نمی‌تواند به تنهایی ثبت شود و باید همراه با یک آیتم دیگر باشد.");

        RuleFor(x => x.PriceUnitId)
            .MustAsync(BeValidPriceUnit)
            .WithMessage("واحد ارزی فاکتور معتبر نمی باشد");

        RuleFor(x => x)
            .MustAsync(NotResultInNegativeInventory)
            .WithMessage("موجودی یک یا چند کالا برای ذخیره فاکتور کافی نیست.");

        RuleForEach(x => x.InvoicePayments)
            .SetValidator(new InvoicePaymentDtoValidator(priceUnitRepository,
                financialAccountRepository,
                paymentVoucherRepository));

        RuleForEach(x => x.InvoiceProductItems)
            .SetValidator(new InvoiceProductItemDtoValidator(
                productCategoryRepository,
                productRepository,
                priceUnitRepository))
            .MustAsync(ProductAvailable)
            .WithMessage("این جنس قبلا فروخته شده است");

        RuleForEach(x => x.InvoiceDiscounts)
            .SetValidator(new InvoiceDiscountDtoValidator(priceUnitRepository));

        RuleForEach(x => x.InvoiceExtraCosts)
            .SetValidator(new InvoiceExtraCostDtoValidator(priceUnitRepository));

        RuleForEach(x => x.InvoiceCoinItems)
            .SetValidator(new InvoiceCoinItemDtoValidator(coinRepository));

        RuleForEach(x => x.InvoiceCurrencyItems)
            .SetValidator(new InvoiceCurrencyItemDtoValidator(priceUnitRepository));

        When(x => !x.Id.HasValue, () =>
        {
            RuleForEach(x => x.InvoiceProductItems)
                .Must(NewProductsHaveCostPrice)
                .WithMessage("وارد کردن نرخ خرید جنس الزامی است.");
        });
    }

    private bool NewProductsHaveCostPrice(InvoiceProductItemDto invoiceProductItem)
    {
        return invoiceProductItem.Product.Id.HasValue || invoiceProductItem.CostPrice.HasValue;
    }

    private async Task<bool> NotResultInNegativeInventory(InvoiceRequestDto request, CancellationToken cancellationToken = default)
    {
        // --- (Create) ---
        if (!request.Id.HasValue)
        {
            if (request.InvoiceType is InvoiceType.Purchase)
                return true;

            foreach (var coinItem in request.InvoiceCoinItems)
            {
                var currentStock = await _inventoryStockRepository.GetQuantityAsync(new CoinId(coinItem.CoinId), cancellationToken);
                if (currentStock < coinItem.Quantity) 
                    return false;
            }

            foreach (var currencyItem in request.InvoiceCurrencyItems)
            {
                var currentStock = await _inventoryStockRepository.GetQuantityAsync(new PriceUnitId(currencyItem.CurrencyId), cancellationToken);
                if (currentStock < currencyItem.Amount)
                    return false;
            }

            foreach (var productItem in request.InvoiceProductItems)
            {
                if (productItem.Product.Id.HasValue)
                {
                    var currentStock = await _inventoryStockRepository.GetQuantityAsync(new ProductId(productItem.Product.Id.Value), cancellationToken);
                    if (currentStock < 1)
                        return false;
                }
            }
        }
        // (Update)
        else
        {

            var originalInvoice = await _invoiceRepository
                .Get(new InvoicesByIdSpecification(new InvoiceId(request.Id.Value)))
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (originalInvoice is null)
                return true;

            var productChanges = new Dictionary<ProductId, decimal>();
            var coinChanges = new Dictionary<CoinId, decimal>();
            var currencyChanges = new Dictionary<PriceUnitId, decimal>();

            foreach (var oldItem in originalInvoice.ProductItems)
            {
                productChanges[oldItem.ProductId] =
                    productChanges.GetValueOrDefault(oldItem.ProductId, 0m) - oldItem.Quantity;
            }

            foreach (var oldItem in originalInvoice.CoinItems)
            {
                coinChanges[oldItem.CoinId] = coinChanges.GetValueOrDefault(oldItem.CoinId, 0m) - oldItem.Quantity;
            }

            foreach (var oldItem in originalInvoice.CurrencyItems)
            {
                currencyChanges[oldItem.CurrencyId] =
                    currencyChanges.GetValueOrDefault(oldItem.CurrencyId, 0m) - oldItem.Amount;
            }

            foreach (var newItem in request.InvoiceProductItems)
            {
                if (newItem.Product.Id.HasValue)
                {
                    var productId = new ProductId(newItem.Product.Id.Value);
                    productChanges[productId] = productChanges.GetValueOrDefault(productId, 0m) + newItem.Quantity;
                }
            }

            foreach (var newItem in request.InvoiceCoinItems)
            {
                var coinId = new CoinId(newItem.CoinId);
                coinChanges[coinId] = coinChanges.GetValueOrDefault(coinId, 0m) + newItem.Quantity;
            }

            foreach (var newItem in request.InvoiceCurrencyItems)
            {
                var currencyId = new PriceUnitId(newItem.CurrencyId);
                currencyChanges[currencyId] = currencyChanges.GetValueOrDefault(currencyId, 0m) + newItem.Amount;
            }

            var allProductIds = productChanges.Keys.ToList();
            var allCoinIds = coinChanges.Keys.ToList();
            var allCurrencyIds = currencyChanges.Keys.ToList();

            var productQuantities =
                await _inventoryStockRepository.GetQuantitiesAsync(allProductIds, cancellationToken);
            var coinQuantities = await _inventoryStockRepository.GetQuantitiesAsync(allCoinIds, cancellationToken);
            var currencyQuantities =
                await _inventoryStockRepository.GetQuantitiesAsync(allCurrencyIds, cancellationToken);

            foreach (var (productId, netChange) in productChanges)
            {
                var currentStock = productQuantities.GetValueOrDefault(productId, 0m);
                if (currentStock + netChange < 0)
                {
                    return false;
                }
            }

            foreach (var (coinId, netChange) in coinChanges)
            {
                var currentStock = coinQuantities.GetValueOrDefault(coinId, 0m);
                if (currentStock + netChange < 0)
                {
                    return false;
                }
            }

            foreach (var (currencyId, netChange) in currencyChanges)
            {
                var currentStock = currencyQuantities.GetValueOrDefault(currencyId, 0m);
                if (currentStock + netChange < 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private async Task<bool> ProductAvailable(InvoiceRequestDto invoiceDto, InvoiceProductItemDto invoiceProductItem, CancellationToken cancellationToken = default)
    {
        if (!invoiceProductItem.Product.Id.HasValue)
            return true;

        var productId = new ProductId(invoiceProductItem.Product.Id.Value);

        if (invoiceDto.Id.HasValue)
        {
            var existingInvoice = await _invoiceRepository
                .Get(new InvoicesByIdSpecification(new InvoiceId(invoiceDto.Id.Value)))
                .FirstOrDefaultAsync(cancellationToken);

            if (existingInvoice != null && existingInvoice.ProductItems.Any(item => item.ProductId == productId))
                return true;
        }
        else
        {
            var quantity = await _inventoryStockRepository.GetQuantityAsync(productId, cancellationToken);

            if (quantity <= 0)
                return false;
        }

        return true;
    }

    private async Task<bool> BeValidPriceUnit(Guid id, CancellationToken cancellationToken = default)
    {
        return await _priceUnitRepository.ExistsAsync(new PriceUnitsByIdSpecification(new PriceUnitId(id)), cancellationToken);
    }

    private async Task<bool> BeUniqueNumber(InvoiceRequestDto request, long invoiceNumber, CancellationToken cancellationToken = default)
    {
        var item = await _invoiceRepository
            .Get(new InvoicesByNumberSpecification(invoiceNumber, request.InvoiceType))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return true;

        return item.Id.Value == request.Id;
    }
}