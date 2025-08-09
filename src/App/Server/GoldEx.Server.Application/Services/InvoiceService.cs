using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.Invoices;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.InvoiceItemProductAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Shared.Constants;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Infrastructure.Specifications.InvoicePayments;
using GoldEx.Server.Infrastructure.Specifications.InvoiceProductItems;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InvoiceService(
    IInvoiceRepository invoiceRepository,
    IInvoiceProductItemRepository invoiceProductItemRepository,
    IInvoicePaymentRepository invoicePaymentRepository,
    ILedgerAccountRepository ledgerAccountRepository,
    IProductRepository productRepository,
    ICustomerService customerService,
    IAccountingTransactionService transactionService,
    IMapper mapper,
    ILogger<InvoiceService> logger,
    InvoiceRequestDtoValidator validator) : IInvoiceService
{
    public async Task SetAsync(InvoiceRequestDto request, CancellationToken cancellationToken)
    {
        await using var dbTransaction = await invoiceRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted,
            cancellationToken);
        {
            try
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                Guid customerId;

                if (request.Customer.Id.HasValue)
                {
                    await customerService.UpdateAsync(request.Customer.Id.Value, request.Customer, cancellationToken);
                    customerId = request.Customer.Id.Value;
                }
                else
                {
                    customerId = await customerService.CreateAsync(request.Customer, cancellationToken);
                }

                var parentAccountTitle = request.InvoiceType == InvoiceType.Sell
                    ? SystemLedgerAccounts.AccountsReceivable 
                    : SystemLedgerAccounts.AccountsPayable;

                var parentLedgerAccount = await ledgerAccountRepository
                                              .Get(new LedgerAccountsByTitleSpecification(parentAccountTitle))
                                              .FirstOrDefaultAsync(cancellationToken)
                                          ?? throw new InvalidOperationException($"System ledger account '{parentAccountTitle}' not found.");

                var existingLedgerAccount = await ledgerAccountRepository
                    .Get(new LedgerAccountsByCustomerAndParentSpecification(new CustomerId(customerId), parentLedgerAccount.Id))
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingLedgerAccount is null)
                {
                    var customer = await customerService.GetAsync(customerId, cancellationToken)
                                   ?? throw new NotFoundException("Customer not found after creation.");

                    var ledgerAccountType = parentLedgerAccount.AccountType;
                    
                    var ledgerAccountTitle = $"{parentLedgerAccount.Title} - {customer.FullName}";
                    var newLedgerAccount = LedgerAccount.CreateCustomerAccount(
                        ledgerAccountTitle,
                        new CustomerId(customer.Id),
                        ledgerAccountType,
                        parentLedgerAccount.Id);

                    await ledgerAccountRepository.CreateAsync(newLedgerAccount, cancellationToken);
                }

                Invoice invoice;
                var isNewInvoice = !request.Id.HasValue;

                if (isNewInvoice)
                {
                    invoice = Invoice.Create(request.InvoiceNumber,
                        request.UnpaidAmountExchangeRate,
                        request.ExchangeRate,
                        request.InvoiceType,
                        new CustomerId(customerId),
                        new PriceUnitId(request.PriceUnitId),
                        request.UnpaidPriceUnitId.HasValue ? new PriceUnitId(request.UnpaidPriceUnitId.Value) : null,
                        DateOnly.FromDateTime(request.InvoiceDate),
                        request.DueDate.HasValue
                            ? DateOnly.FromDateTime(request.DueDate.Value)
                            : null);

                    await invoiceRepository.CreateAsync(invoice, cancellationToken);
                }
                else
                {
                    invoice = await invoiceRepository.Get(new InvoicesByIdSpecification(new InvoiceId(request.Id!.Value)))
                        .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                    invoice.SetPriceUnitId(new PriceUnitId(request.PriceUnitId));
                    invoice.SetCustomerId(new CustomerId(customerId));
                    invoice.SetInvoiceDate(DateOnly.FromDateTime(request.InvoiceDate));
                    invoice.SetDueDate(request.DueDate.HasValue ? DateOnly.FromDateTime(request.DueDate.Value) : null);
                    invoice.SetInvoiceNumber(request.InvoiceNumber);
                    invoice.SetExchangeRate(request.ExchangeRate);
                    invoice.SetUnpaidAmountExchangeRate(request.UnpaidAmountExchangeRate);
                    invoice.SetUnpaidPriceUnitId(request.UnpaidPriceUnitId.HasValue 
                        ? new PriceUnitId(request.UnpaidPriceUnitId.Value) 
                        : null);
                }

                invoice.SetDiscounts(request.InvoiceDiscounts.Select(x =>
                    InvoiceDiscount.Create(x.Amount, x.ExchangeRate, new PriceUnitId(x.PriceUnitId), x.Description)));

                invoice.SetExtraCosts(request.InvoiceExtraCosts.Select(x =>
                    InvoiceExtraCost.Create(x.Amount, x.ExchangeRate, new PriceUnitId(x.PriceUnitId), x.Description)));

                var existingPayments = request.Id.HasValue
                    ? await invoicePaymentRepository.Get(new InvoicePaymentsByInvoiceIdSpecification(invoice.Id))
                        .ToListAsync(cancellationToken)
                    : [];

                var paymentDtos = request.InvoicePayments.ToList();

                var paymentsToDelete = existingPayments
                    .Where(ep => paymentDtos.All(dto => dto.Id != ep.Id.Value))
                    .ToList();

                if (paymentsToDelete.Any()) 
                    await invoicePaymentRepository.DeleteRangeAsync(paymentsToDelete, cancellationToken);

                var paymentsToCreate = paymentDtos
                    .Where(dto => !dto.Id.HasValue)
                    .Select(dto => InvoicePayment.Create(
                        dto.PaymentDate,
                        dto.Amount,
                        dto.ExchangeRate,
                        invoice.Id, 
                        new PriceUnitId(dto.PriceUnitId),
                        dto.FinancialAccountId.HasValue ? new FinancialAccountId(dto.FinancialAccountId.Value) : null,
                        dto.VoucherId.HasValue ? new PaymentVoucherId(dto.VoucherId.Value) : null,
                        dto.ReferenceNumber,
                        dto.Note))
                    .ToList();

                if (paymentsToCreate.Any()) 
                    await invoicePaymentRepository.CreateRangeAsync(paymentsToCreate, cancellationToken);

                var paymentsToUpdate = new List<InvoicePayment>();
                var paymentsToUpdateDtos = paymentDtos.Where(dto => dto.Id.HasValue);

                foreach (var dto in paymentsToUpdateDtos)
                {
                    var existingPayment = existingPayments.FirstOrDefault(p => p.Id.Value == dto.Id!.Value)
                        ?? throw new NotFoundException("InvoicePayment not found for update.");

                    existingPayment.SetPaymentDate(dto.PaymentDate);
                    existingPayment.SetAmount(dto.Amount, new PriceUnitId(dto.PriceUnitId));
                    existingPayment.SetSourceFinancialAccountId(dto.FinancialAccountId.HasValue ? new FinancialAccountId(dto.FinancialAccountId.Value) : null);
                    existingPayment.SetPaymentVoucherId(dto.VoucherId.HasValue ? new PaymentVoucherId(dto.VoucherId.Value) : null);
                    existingPayment.SetReferenceNumber(dto.ReferenceNumber);
                    existingPayment.SetNote(dto.Note);

                    paymentsToUpdate.Add(existingPayment);
                }

                if (paymentsToUpdate.Any())
                {
                    await invoicePaymentRepository.UpdateRangeAsync(paymentsToUpdate, cancellationToken);
                }

                if (!isNewInvoice) 
                    await invoiceRepository.UpdateAsync(invoice, cancellationToken);

                var existingProductItems = request.Id.HasValue
                    ? await invoiceProductItemRepository.Get(new InvoiceProductItemsByInvoiceIdSpecification(invoice.Id))
                        .ToListAsync(cancellationToken) 
                    : [];

                foreach (var itemDto in request.InvoiceProductItems)
                {
                    Product product;

                    if (!itemDto.Product.Id.HasValue)
                    {
                        var newProduct = Product.Create(itemDto.Product.Name,
                            itemDto.Product.Barcode,
                            itemDto.Product.Weight,
                            itemDto.Product.Wage,
                            itemDto.Product.ProductType,
                            itemDto.Product.CaratType,
                            itemDto.Product.GoldUnitType,
                            itemDto.Product.WageType,
                            itemDto.Product.WagePriceUnitId.HasValue
                                ? new PriceUnitId(itemDto.Product.WagePriceUnitId.Value)
                                : null,
                            itemDto.Product.ProductCategoryId.HasValue
                                ? new ProductCategoryId(itemDto.Product.ProductCategoryId.Value)
                                : null);

                        await productRepository.CreateAsync(newProduct, cancellationToken);

                        product = newProduct;
                    }
                    else
                    {
                        var existingProduct = await productRepository
                            .Get(new ProductsByIdSpecification(new ProductId(itemDto.Product.Id.Value)))
                            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                        existingProduct.SetName(itemDto.Product.Name);
                        existingProduct.SetBarcode(itemDto.Product.Barcode);
                        existingProduct.SetWeight(itemDto.Product.Weight);
                        existingProduct.SetCaratType(itemDto.Product.CaratType);
                        existingProduct.SetGoldUnitType(itemDto.Product.GoldUnitType);
                        existingProduct.SetProductType(itemDto.Product.ProductType);
                        existingProduct.SetWage(itemDto.Product.Wage);
                        existingProduct.SetWageType(itemDto.Product.WageType);
                        existingProduct.SetProductCategory(
                            itemDto.Product.ProductCategoryId.HasValue
                                ? new ProductCategoryId(itemDto.Product.ProductCategoryId.Value)
                                : null);
                        existingProduct.SetWagePriceUnitId(
                            itemDto.Product.WagePriceUnitId.HasValue
                                ? new PriceUnitId(itemDto.Product.WagePriceUnitId.Value)
                                : null);
                        if (itemDto.Product.ProductType == ProductType.Jewelry)
                        {
                            existingProduct.SetGemStones(itemDto.Product.GemStones?.Select(s => GemStone.Create(s.Code,
                                s.Type,
                                s.Color,
                                s.Cut,
                                s.Carat,
                                s.Purity,
                                existingProduct.Id)));
                        }
                        else
                            existingProduct.ClearGemStones();

                        await productRepository.UpdateAsync(existingProduct, cancellationToken);

                        product = existingProduct;
                    }

                    var existingItem = request.InvoiceType == InvoiceType.Sell
                        ? existingProductItems.FirstOrDefault(x => x.SellProductId == product.Id)
                        : existingProductItems.FirstOrDefault(x => x.PurchaseProductId == product.Id);

                    if (existingItem != null)
                    {
                        existingItem.SetGramPrice(itemDto.GramPrice);
                        existingItem.SetQuantity(itemDto.Quantity);
                        existingItem.SetProfitPercent(itemDto.ProfitPercent);
                        existingItem.SetTaxPercent(itemDto.TaxPercent);
                        existingItem.SetExchangeRate(itemDto.ExchangeRate);
                        existingItem.SetPriceUnitId(new PriceUnitId(itemDto.PriceUnit));

                        existingItem.RecalculateAmounts(product);

                        await invoiceProductItemRepository.UpdateAsync(existingItem, cancellationToken);
                        existingProductItems.Remove(existingItem);
                    }
                    else
                    {
                        var invoiceProductItem = InvoiceProductItem.Create(
                            itemDto.GramPrice,
                            itemDto.ProfitPercent,
                            itemDto.TaxPercent,
                            itemDto.Quantity,
                            product.Id,
                            request.InvoiceType,
                            new PriceUnitId(itemDto.PriceUnit),
                            invoice.Id,
                            itemDto.ExchangeRate);

                        invoiceProductItem.RecalculateAmounts(product);

                        await invoiceProductItemRepository.CreateAsync(invoiceProductItem, cancellationToken);
                    }
                }

                foreach (var itemToDelete in existingProductItems)
                {
                    await invoiceProductItemRepository.DeleteAsync(itemToDelete, cancellationToken);
                }

                await transactionService.CreateTransactionsForInvoiceAsync(invoice, cancellationToken);

                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public async Task<PagedList<GetInvoiceListResponse>> GetListAsync(RequestFilter filter, InvoiceFilter invoiceFilter,
        Guid? customerId, CancellationToken cancellationToken = default)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        var spec = new InvoicesByFilterSpecification(
            filter,
            invoiceFilter,
            customerId.HasValue ? new CustomerId(customerId.Value) : null
        );

        var data = await invoiceRepository
            .Get(spec)
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.SellProduct)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var totalCount = await invoiceRepository.CountAsync(spec, cancellationToken);

        return new PagedList<GetInvoiceListResponse>
        {
            Data = mapper.Map<List<GetInvoiceListResponse>>(data),
            Skip = skip,
            Take = take,
            Total = totalCount
        };
    }

    public async Task<GetInvoiceResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await invoiceRepository
            .Get(new InvoicesByIdSpecification(new InvoiceId(id)))
            .Include(x => x.Customer!)
                .ThenInclude(x => x.CreditLimitPriceUnit)
            .Include(x => x.PriceUnit)
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.SellProduct)
                    .ThenInclude(x => x!.ProductCategory)
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.PurchaseProduct)
                    .ThenInclude(x => x!.ProductCategory)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.PriceUnit)
            .Include(x => x.UnpaidPriceUnit)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.SourceFinancialAccount)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetInvoiceResponse>(item);
    }

    public async Task<GetInvoiceResponse> GetAsync(long invoiceNumber, InvoiceType invoiceType,
        CancellationToken cancellationToken = default)
    {
        var item = await invoiceRepository
            .Get(new InvoicesByNumberSpecification(invoiceNumber, invoiceType))
            .Include(x => x.Customer!)
                .ThenInclude(x => x.CreditLimitPriceUnit)
            .Include(x => x.PriceUnit)
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.SellProduct)
                    .ThenInclude(x => x!.ProductCategory)
            .Include(x => x.ProductItems)
                .ThenInclude(x => x.PurchaseProduct)
                    .ThenInclude(x => x!.ProductCategory)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.PriceUnit)
            .Include(x => x.UnpaidPriceUnit)
            .Include(x => x.InvoicePayments!)
                .ThenInclude(x => x.SourceFinancialAccount)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetInvoiceResponse>(item);
    }

    public async Task DeleteAsync(Guid id, bool deleteProducts, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await invoiceRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                var item = await invoiceRepository
                    .Get(new InvoicesByIdSpecification(new InvoiceId(id)))
                    .Include(x => x.ProductItems)
                        .ThenInclude(x => x.SellProduct)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                var products = item.ProductItems.Select(x => x.SellProduct!).ToList();

                await invoiceRepository.DeleteAsync(item, cancellationToken);

                if (deleteProducts) 
                    await productRepository.DeleteRangeAsync(products, cancellationToken);

                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public async Task<GetInvoiceNumberResponse> GetLastNumberAsync(InvoiceType invoiceType,
        CancellationToken cancellationToken = default)
    {
        var number = await invoiceRepository.GetLastNumberAsync(invoiceType, cancellationToken);

        return new GetInvoiceNumberResponse(number);
    }
}