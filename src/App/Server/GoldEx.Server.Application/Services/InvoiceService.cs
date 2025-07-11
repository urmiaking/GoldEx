using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.Invoices;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoiceItemAggregate;
using GoldEx.Server.Domain.PaymentMethodAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using GoldEx.Server.Infrastructure.Specifications.InvoiceItems;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InvoiceService(
    IInvoiceRepository invoiceRepository,
    IInvoiceItemRepository invoiceItemRepository,
    IProductRepository productRepository,
    ICustomerService customerService,
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

                Invoice invoice;
                var isNewInvoice = !request.Id.HasValue;

                if (isNewInvoice)
                {
                    invoice = Invoice.Create(request.InvoiceNumber,
                        request.UnpaidAmountExchangeRate,
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
                    invoice.SetUnpaidAmountExchangeRate(request.UnpaidAmountExchangeRate);
                    invoice.SetUnpaidPriceUnitId(request.UnpaidPriceUnitId.HasValue 
                        ? new PriceUnitId(request.UnpaidPriceUnitId.Value) 
                        : null);
                }

                invoice.SetDiscounts(request.InvoiceDiscounts.Select(x =>
                    InvoiceDiscount.Create(x.Amount, x.ExchangeRate, new PriceUnitId(x.PriceUnitId), x.Description)));

                invoice.SetExtraCosts(request.InvoiceExtraCosts.Select(x =>
                    InvoiceExtraCost.Create(x.Amount, x.ExchangeRate, new PriceUnitId(x.PriceUnitId), x.Description)));

                invoice.SetInvoicePayments(request.InvoicePayments.Select(x =>
                    InvoicePayment.Create(x.PaymentDate,
                        x.Amount,
                        x.ExchangeRate,
                        new PriceUnitId(x.PriceUnitId),
                        new PaymentMethodId(x.PaymentMethodId),
                        x.ReferenceNumber,
                        x.Note)));

                if (!isNewInvoice) 
                    await invoiceRepository.UpdateAsync(invoice, cancellationToken);

                var existingItems = request.Id.HasValue
                    ? await invoiceItemRepository.Get(new InvoiceItemsByInvoiceIdSpecification(invoice.Id))
                        .ToListAsync(cancellationToken) 
                    : [];

                foreach (var itemDto in request.InvoiceItems)
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
                            itemDto.Product.WageType,
                            itemDto.Product.WagePriceUnitId.HasValue
                                ? new PriceUnitId(itemDto.Product.WagePriceUnitId.Value)
                                : null,
                            itemDto.Product.ProductCategoryId.HasValue
                                ? new ProductCategoryId(itemDto.Product.ProductCategoryId.Value)
                                : null);

                        newProduct.MarkAsSold();

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

                        existingProduct.MarkAsSold();

                        await productRepository.UpdateAsync(existingProduct, cancellationToken);

                        product = existingProduct;
                    }

                    var existingItem = existingItems.FirstOrDefault(x => x.ProductId == product.Id);

                    if (existingItem != null)
                    {
                        existingItem.SetGramPrice(itemDto.GramPrice);
                        existingItem.SetQuantity(itemDto.Quantity);
                        existingItem.SetProfitPercent(itemDto.ProfitPercent);
                        existingItem.SetTaxPercent(itemDto.TaxPercent);
                        existingItem.SetExchangeRate(itemDto.ExchangeRate);
                        existingItem.SetPriceUnitId(new PriceUnitId(itemDto.PriceUnit));

                        existingItem.RecalculateAmounts(product);

                        await invoiceItemRepository.UpdateAsync(existingItem, cancellationToken);
                        existingItems.Remove(existingItem);
                    }
                    else
                    {
                        var invoiceItem = InvoiceItem.Create(itemDto.GramPrice,
                            itemDto.ProfitPercent,
                            itemDto.TaxPercent,
                            itemDto.Quantity,
                            product.Id,
                            new PriceUnitId(itemDto.PriceUnit),
                            invoice.Id,
                            itemDto.ExchangeRate);

                        invoiceItem.RecalculateAmounts(product);

                        await invoiceItemRepository.CreateAsync(invoiceItem, cancellationToken);
                    }
                }

                foreach (var itemToDelete in existingItems)
                {
                    await invoiceItemRepository.DeleteAsync(itemToDelete, cancellationToken);
                }

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

    public async Task<PagedList<GetInvoiceListResponse>> GetListAsync(RequestFilter filter, Guid? customerId, CancellationToken cancellationToken = default)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        var data = await invoiceRepository
            .Get(new InvoicesByFilterSpecification(filter,
                customerId.HasValue
                    ? new CustomerId(customerId.Value)
                    : null))
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var totalCount = await invoiceRepository.CountAsync(new InvoicesByFilterSpecification(filter,
                customerId.HasValue
                    ? new CustomerId(customerId.Value)
                    : null),
            cancellationToken);

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
            .Include(x => x.Customer)
                .ThenInclude(x => x.CreditLimitPriceUnit)
            .Include(x => x.PriceUnit)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                    .ThenInclude(x => x!.ProductCategory)
            .Include(x => x.InvoicePayments)
                .ThenInclude(x => x.PriceUnit)
            .Include(x => x.InvoicePayments)    
                .ThenInclude(x => x.PaymentMethod)
            .Include(x => x.UnpaidPriceUnit)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetInvoiceResponse>(item);
    }

    public async Task<GetInvoiceResponse> GetAsync(long invoiceNumber, CancellationToken cancellationToken = default)
    {
        var item = await invoiceRepository
            .Get(new InvoicesByNumberSpecification(invoiceNumber))
            .Include(x => x.Customer)
                .ThenInclude(x => x.CreditLimitPriceUnit)
            .Include(x => x.PriceUnit)
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                    .ThenInclude(x => x!.ProductCategory)
            .Include(x => x.InvoicePayments)
                .ThenInclude(x => x.PriceUnit)
            .Include(x => x.InvoicePayments)
                .ThenInclude(x => x.PaymentMethod)
            .Include(x => x.UnpaidPriceUnit)
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
                    .Include(x => x.Items)
                        .ThenInclude(x => x.Product)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                var products = item.Items.Select(x => x.Product!).ToList();

                await invoiceRepository.DeleteAsync(item, cancellationToken);

                if (deleteProducts)
                {
                    await productRepository.DeleteRangeAsync(products, cancellationToken);
                }
                else
                {
                    foreach (var product in products)
                        product.MarkAsAvailable();

                    await productRepository.UpdateRangeAsync(products, cancellationToken);
                }

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

    public async Task<GetInvoiceNumberResponse> GetLastNumberAsync(CancellationToken cancellationToken = default)
    {
        var number = await invoiceRepository.GetLastNumberAsync(cancellationToken);

        return new GetInvoiceNumberResponse(number);
    }
}