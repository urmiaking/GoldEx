using System.Data;
using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Application.Validators.Invoices;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoiceItemAggregate;
using GoldEx.Server.Domain.PaymentMethodAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Server.Infrastructure.Specifications.Transactions;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    public async Task CreateAsync(InvoiceRequestDto request, CancellationToken cancellationToken)
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

                var invoice = Invoice.Create(request.InvoiceNumber,
                    new CustomerId(customerId),
                    new PriceUnitId(request.PriceUnitId),
                    DateOnly.FromDateTime(request.InvoiceDate),
                    request.DueDate.HasValue
                        ? DateOnly.FromDateTime(request.DueDate.Value)
                        : null);

                if (request.InvoiceDiscounts.Any())
                {
                    invoice.SetDiscounts(request.InvoiceDiscounts.Select(x =>
                        InvoiceDiscount.Create(x.Amount, new PriceUnitId(x.PriceUnitId), x.Description)));
                }

                if (request.InvoiceExtraCosts.Any())
                {
                    invoice.SetExtraCosts(request.InvoiceExtraCosts.Select(x =>
                        InvoiceExtraCost.Create(x.Amount, new PriceUnitId(x.PriceUnitId), x.Description)));
                }

                if (request.InvoicePayments.Any())
                {
                    invoice.SetInvoicePayments(request.InvoicePayments.Select(x =>
                        InvoicePayment.Create(x.PaymentDate,
                            x.Amount,
                            new PriceUnitId(x.PriceUnitId),
                            new PaymentMethodId(x.PaymentMethodId),
                            x.ReferenceNumber,
                            x.Note)));
                }

                await invoiceRepository.CreateAsync(invoice, cancellationToken);

                foreach (var invoiceItemRequest in request.InvoiceItems)
                {
                    var productRequest = invoiceItemRequest.Product;

                    ProductId productId;

                    if (!productRequest.Id.HasValue)
                    {
                        var product = Product.Create(productRequest.Name,
                            productRequest.Barcode,
                            productRequest.Weight,
                            productRequest.Wage,
                            productRequest.ProductType,
                            productRequest.CaratType,
                            productRequest.WageType,
                            productRequest.WagePriceUnitId.HasValue
                                ? new PriceUnitId(productRequest.WagePriceUnitId.Value)
                                : null,
                            productRequest.ProductCategoryId.HasValue
                                ? new ProductCategoryId(productRequest.ProductCategoryId.Value)
                                : null);

                        await productRepository.CreateAsync(product, cancellationToken);

                        productId = product.Id;
                    }
                    else
                    {
                        var product = await productRepository
                            .Get(new ProductsByIdSpecification(new ProductId(productRequest.Id.Value)))
                            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                        product.SetName(productRequest.Name);
                        product.SetBarcode(productRequest.Barcode);
                        product.SetWeight(productRequest.Weight);
                        product.SetCaratType(productRequest.CaratType);
                        product.SetProductType(productRequest.ProductType);
                        product.SetWage(productRequest.Wage);
                        product.SetWageType(productRequest.WageType);
                        product.SetProductCategory(
                            productRequest.ProductCategoryId.HasValue
                                ? new ProductCategoryId(productRequest.ProductCategoryId.Value)
                                : null);
                        product.SetWagePriceUnitId(
                            productRequest.WagePriceUnitId.HasValue
                                ? new PriceUnitId(productRequest.WagePriceUnitId.Value)
                                : null);
                        if (productRequest.ProductType == ProductType.Jewelry)
                        {
                            product.SetGemStones(productRequest.GemStones?.Select(s => GemStone.Create(s.Code,
                                s.Type,
                                s.Color,
                                s.Cut,
                                s.Carat,
                                s.Purity,
                                product.Id)));
                        }
                        else
                            product.ClearGemStones();

                        productId = product.Id;
                    }

                    var invoiceItem = InvoiceItem.Create(invoiceItemRequest.GramPrice,
                        invoiceItemRequest.ProfitPercent,
                        invoiceItemRequest.TaxPercent,
                        invoiceItemRequest.Quantity,
                        productId,
                        new PriceUnitId(invoiceItemRequest.PriceUnit),
                        invoice.Id,
                        invoiceItemRequest.ExchangeRate);

                    await invoiceItemRepository.CreateAsync(invoiceItem, cancellationToken);
                }

                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                await dbTransaction.RollbackAsync(cancellationToken);
            }
        }
    }

    public async Task<PagedList<GetInvoiceResponse>> GetListAsync(RequestFilter filter, Guid? customerId, CancellationToken cancellationToken = default)
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

        return new PagedList<GetInvoiceResponse>
        {
            Data = mapper.Map<List<GetInvoiceResponse>>(data),
            Skip = skip,
            Take = take,
            Total = totalCount
        };
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}