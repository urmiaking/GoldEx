using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Server.Infrastructure.Specifications.Transactions;
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
    private readonly IProductRepository _productRepository;
    private readonly IFinancialAccountRepository _financialAccountRepository;
    private readonly ITransactionRepository _transactionRepository;

    public InvoiceRequestDtoValidator(ICustomerRepository customerRepository,
        IPriceUnitRepository priceUnitRepository, IProductRepository productRepository, 
        IFinancialAccountRepository financialAccountRepository, IInvoiceRepository invoiceRepository, IPaymentVoucherRepository paymentVoucherRepository,
        ICoinRepository coinRepository, IInventoryStockRepository inventoryStockRepository, ITransactionRepository transactionRepository)
    {
        _priceUnitRepository = priceUnitRepository;
        _productRepository = productRepository;
        _financialAccountRepository = financialAccountRepository;
        _invoiceRepository = invoiceRepository;
        _inventoryStockRepository = inventoryStockRepository;
        _transactionRepository = transactionRepository;

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
            .WithMessage("فاکتور باید حداقل دارای یک آیتم (کالا، ارز، سکه یا جنس مستعمل) باشد.");

        RuleFor(x => x)
            .Must(invoice =>
                invoice.InvoiceProductItems.Any() ||
                invoice.InvoiceCurrencyItems.Any() ||
                invoice.InvoiceCoinItems.Any())
            .When(invoice =>
                invoice.InvoiceType == InvoiceType.Sell &&
                invoice.InvoiceUsedProducts.Any())
            .WithMessage("در فاکتور فروش، کالای مستعمل نمی‌تواند به تنهایی ثبت شود و باید همراه با یک آیتم دیگر باشد.");

        RuleFor(x => x.PriceUnitId)
            .MustAsync(BeValidPriceUnit)
            .WithMessage("واحد ارزی فاکتور معتبر نمی باشد");

        RuleFor(x => x)
            .MustAsync(NotResultInNegativeInventory)
            .WithMessage("موجودی یک یا چند کالا برای ذخیره فاکتور کافی نیست.");

        RuleForEach(x => x.InvoicePayments)
            .SetValidator(new InvoicePaymentDtoValidator(priceUnitRepository,
                financialAccountRepository,
                paymentVoucherRepository,
                customerRepository));

        RuleFor(x => x)
            .MustAsync(NotResultInNegativeMoltenGoldInventory)
            .WithMessage("موجودی طلای آبشده برای تسویه کافی نیست.");

        RuleForEach(x => x.InvoiceProductItems)
            .SetValidator(new InvoiceProductItemDtoValidator(
                productRepository,
                priceUnitRepository))
            .MustAsync(ProductAvailable)
            .WithMessage("این جنس قبلا فروخته شده است");

        RuleFor(x => x)
            .MustAsync(NotResultInNegativeLedgerBalances)
            .When(x => x.InvoiceType == InvoiceType.Sell && x.InvoiceCurrencyItems.Any())
            .WithMessage("موجودی حساب مالی ارز کافی نیست.");

        RuleForEach(x => x.InvoiceDiscounts)
            .SetValidator(new InvoiceDiscountDtoValidator(priceUnitRepository));

        RuleForEach(x => x.InvoiceExtraCosts)
            .SetValidator(new InvoiceExtraCostDtoValidator(priceUnitRepository));

        RuleForEach(x => x.InvoiceCoinItems)
            .SetValidator(new InvoiceCoinItemDtoValidator(coinRepository, customerRepository));

        RuleForEach(x => x.InvoiceCurrencyItems)
            .SetValidator(new InvoiceCurrencyItemDtoValidator(priceUnitRepository));

        When(x => !x.Id.HasValue, () =>
        {
            RuleForEach(x => x.InvoiceProductItems)
                .Must(NewProductsHaveCostPrice)
                .WithMessage("وارد کردن نرخ خرید جنس الزامی است.");
        });
    }

    private async Task<bool> NotResultInNegativeLedgerBalances(InvoiceRequestDto request, CancellationToken cancellationToken)
    {
        if (request.InvoiceType != InvoiceType.Sell)
            return true;

        if (!request.Id.HasValue)
        {
            foreach (var currencyItem in request.InvoiceCurrencyItems)
            {
                var fin = await _financialAccountRepository
                    .Get(new FinancialAccountsByIdSpecification(new FinancialAccountId(currencyItem.FinancialAccountId)))
                    .FirstOrDefaultAsync(cancellationToken);

                if (fin?.LedgerAccountId == null)
                    return false;

                var ledgerId = fin.LedgerAccountId.Value;

                var currentBalance = await GetActiveLedgerBalanceAsync(ledgerId, cancellationToken);

                if (currentBalance < currencyItem.Amount)
                    return false;
            }

            return true;
        }

        // Update
        var originalInvoice = await _invoiceRepository
            .Get(new InvoicesByIdSpecification(new InvoiceId(request.Id.Value)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (originalInvoice is null)
            return true;

        // key: (LedgerAccountId, PriceUnitId)
        var oldOut = new Dictionary<LedgerAccountId, decimal>();
        var newOut = new Dictionary<LedgerAccountId, decimal>();

        // قدیمی‌ها: ممکن است فیلد FinancialAccountId قبلاً اجباری نبوده باشد
        foreach (var oldCi in originalInvoice.CurrencyItems)
        {
            if (!oldCi.FinancialAccountId.HasValue)
                continue;

            var fin = await _financialAccountRepository
                .Get(new FinancialAccountsByIdSpecification(oldCi.FinancialAccountId.Value))
                .FirstOrDefaultAsync(cancellationToken);

            if (fin?.LedgerAccountId == null)
                continue;

            var key = fin.LedgerAccountId.Value;
            oldOut[key] = oldOut.GetValueOrDefault(key, 0m) + oldCi.Amount;
        }

        // جدیدها
        foreach (var newCi in request.InvoiceCurrencyItems)
        {
            var fin = await _financialAccountRepository
                .Get(new FinancialAccountsByIdSpecification(new FinancialAccountId(newCi.FinancialAccountId)))
                .FirstOrDefaultAsync(cancellationToken);

            if (fin?.LedgerAccountId == null)
                return false;

            var key = fin.LedgerAccountId.Value;
            newOut[key] = newOut.GetValueOrDefault(key, 0m) + newCi.Amount;
        }

        // بررسی بر اساس Δ خروج = new - old
        var allKeys = newOut.Keys.Union(oldOut.Keys).ToList();
        foreach (var key in allKeys)
        {
            var newAmt = newOut.GetValueOrDefault(key, 0m);
            var oldAmt = oldOut.GetValueOrDefault(key, 0m);
            var deltaOut = newAmt - oldAmt; // نیاز خروج اضافه نسبت به قبل

            if (deltaOut <= 0)
                continue;

            var currentBalance = await GetActiveLedgerBalanceAsync(key, cancellationToken);
            if (currentBalance < deltaOut)
                return false;
        }

        return true;
    }

    private Task<decimal> GetActiveLedgerBalanceAsync(LedgerAccountId ledgerAccountId, CancellationToken ct)
    {
        return _transactionRepository
            .Get(new TransactionsByLedgerAccountIdSpecification(ledgerAccountId, skipReversed: true))
            .AsNoTracking()
            .Select(t => t.TransactionType == TransactionType.Debit ? t.Amount : -t.Amount)
            .SumAsync(ct);
    }

    private bool NewProductsHaveCostPrice(InvoiceProductItemDto invoiceProductItem)
    {
        return invoiceProductItem.Product.Id.HasValue || invoiceProductItem.CostPrice.HasValue;
    }

    private async Task<bool> NotResultInNegativeInventory(InvoiceRequestDto request, CancellationToken cancellationToken = default)
    {
        // -------------------------------------------------
        // CREATE
        // -------------------------------------------------
        if (!request.Id.HasValue)
        {
            if (request.InvoiceType == InvoiceType.Purchase)
                return true;

            // Products
            foreach (var item in request.InvoiceProductItems)
            {
                if (!item.Product.Id.HasValue)
                    continue;

                var productId = new ProductId(item.Product.Id.Value);
                var currentStock =
                    await _inventoryStockRepository.GetQuantityAsync(productId, cancellationToken);

                if (currentStock < item.TotalWeight)
                    return false;
            }

            // Coins
            foreach (var item in request.InvoiceCoinItems)
            {
                if (!item.CoinInstance.Id.HasValue)
                    continue;

                var coinId = new CoinInstanceId(item.CoinInstance.Id.Value);
                var currentStock =
                    await _inventoryStockRepository.GetQuantityAsync(coinId, cancellationToken);

                if (currentStock < item.Quantity)
                    return false;
            }

            // Currencies
            foreach (var item in request.InvoiceCurrencyItems)
            {
                var currencyId = new PriceUnitId(item.CurrencyId);
                var currentStock =
                    await _inventoryStockRepository.GetQuantityAsync(currencyId, cancellationToken);

                if (currentStock < item.Amount)
                    return false;
            }

            return true;
        }

        // -------------------------------------------------
        // UPDATE
        // -------------------------------------------------
        var originalInvoice = await _invoiceRepository
            .Get(new InvoicesByIdSpecification(new InvoiceId(request.Id.Value)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (originalInvoice is null)
            return true;

        if (request.InvoiceType == InvoiceType.Purchase)
            return true;

        // ----------------------------
        // PRODUCTS
        // ----------------------------
        var productIds = originalInvoice.ProductItems
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();

        var productStocks =
            await _inventoryStockRepository.GetQuantitiesAsync(productIds, cancellationToken);

        foreach (var productId in productIds)
        {
            var currentStock = productStocks.GetValueOrDefault(productId, 0m);

            var oldQuantity = originalInvoice.ProductItems
                .Where(x => x.ProductId == productId)
                .Sum(x => x.Quantity);

            var newQuantity = request.InvoiceProductItems
                .Where(x => x.Product.Id == productId.Value)
                .Sum(x => x.Quantity);

            var baselineStock = currentStock + oldQuantity;

            if (baselineStock - newQuantity < 0)
                return false;
        }

        // ----------------------------
        // COINS
        // ----------------------------
        var coinIds = originalInvoice.CoinItems
            .Select(x => x.CoinInstanceId)
            .Distinct()
            .ToList();

        var coinStocks =
            await _inventoryStockRepository.GetQuantitiesAsync(coinIds, cancellationToken);

        foreach (var coinId in coinIds)
        {
            var currentStock = coinStocks.GetValueOrDefault(coinId, 0m);

            var oldQuantity = originalInvoice.CoinItems
                .Where(x => x.CoinInstanceId == coinId)
                .Sum(x => x.Quantity);

            var newQuantity = request.InvoiceCoinItems
                .Where(x => x.Id == coinId.Value)
                .Sum(x => x.Quantity);

            var baselineStock = currentStock + oldQuantity;

            if (baselineStock - newQuantity < 0)
                return false;
        }

        // ----------------------------
        // CURRENCIES
        // ----------------------------
        var currencyIds = originalInvoice.CurrencyItems
            .Select(x => x.CurrencyId)
            .Distinct()
            .ToList();

        var currencyStocks =
            await _inventoryStockRepository.GetQuantitiesAsync(currencyIds, cancellationToken);

        foreach (var currencyId in currencyIds)
        {
            var currentStock = currencyStocks.GetValueOrDefault(currencyId, 0m);

            var oldAmount = originalInvoice.CurrencyItems
                .Where(x => x.CurrencyId == currencyId)
                .Sum(x => x.Amount);

            var newAmount = request.InvoiceCurrencyItems
                .Where(x => x.CurrencyId == currencyId.Value)
                .Sum(x => x.Amount);

            var baselineStock = currentStock + oldAmount;

            if (baselineStock - newAmount < 0)
                return false;
        }

        return true;
    }
    private async Task<bool> NotResultInNegativeMoltenGoldInventory(
    InvoiceRequestDto request,
    CancellationToken cancellationToken = default)
    {
        // فقط فاکتور فروش
        if (request.InvoiceType != InvoiceType.Purchase)
            return true;

        // جمع‌بندی تغییرات برای حالت Update
        var moltenGoldChanges = new Dictionary<ProductId, decimal>(); // ProductId -> netChange

        if (request.Id.HasValue)
        {
            // حالت Update: موجودی قبلی فاکتور
            var originalInvoice = await _invoiceRepository
                .Get(new InvoicesByIdSpecification(new InvoiceId(request.Id.Value)))
                .Include(x => x.InvoicePayments)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (originalInvoice is null)
                return true;

            foreach (var oldPayment in originalInvoice.InvoicePayments!
                         .Where(p => p.PaymentType == PaymentType.MoltenGoldInventory))
            {
                if (!oldPayment.GoldFineness.HasValue)
                    continue;

                var product = await _productRepository
                    .Get(new ProductsByMoltenGoldSpecification(oldPayment.GoldFineness.Value))
                    .FirstOrDefaultAsync(cancellationToken);

                if (product != null)
                    moltenGoldChanges[product.Id] = moltenGoldChanges.GetValueOrDefault(product.Id, 0m) - oldPayment.Amount;
            }
        }

        // پرداخت‌های جدید/ویرایش شده
        foreach (var payment in request.InvoicePayments
                     .Where(p => p.PaymentType == PaymentType.MoltenGoldInventory))
        {
            if (!payment.GoldFineness.HasValue)
                throw new InvalidOperationException("عیار طلای آبشده برای پرداخت الزامی است.");

            var product = await _productRepository
                .Get(new ProductsByMoltenGoldSpecification(payment.GoldFineness.Value))
                .FirstOrDefaultAsync(cancellationToken);

            if (product is null)
                return false; // طلای آبشده با این عیار وجود ندارد

            moltenGoldChanges[product.Id] = moltenGoldChanges.GetValueOrDefault(product.Id, 0m) + payment.Amount;
        }

        // بررسی موجودی خالص
        foreach (var (productId, netChange) in moltenGoldChanges)
        {
            var currentStock = await _inventoryStockRepository.GetQuantityAsync(productId, cancellationToken);
            if (currentStock + netChange < 0)
                return false; // موجودی کافی نیست
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