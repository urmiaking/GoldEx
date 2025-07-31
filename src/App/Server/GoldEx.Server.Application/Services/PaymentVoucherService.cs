using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.PaymentVouchers;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PaymentVouchers;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class PaymentVoucherService(
    IPaymentVoucherRepository repository,
    IMapper mapper,
    PaymentVoucherRequestDtoValidator validator,
    DeletePaymentVoucherValidator deleteValidator) : IPaymentVoucherService
{
    public async Task<PagedList<GetPaymentVoucherListResponse>> GetListAsync(RequestFilter filter, PaymentVoucherFilter voucherFilter, Guid? customerId,
        CancellationToken cancellationToken = default)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        var spec = new PaymentVouchersByFilterSpecification(
            filter,
            voucherFilter,
            customerId.HasValue ? new CustomerId(customerId.Value) : null
        );

        var data = await repository
            .Get(spec)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var totalCount = await repository.CountAsync(spec, cancellationToken);

        return new PagedList<GetPaymentVoucherListResponse>
        {
            Data = mapper.Map<List<GetPaymentVoucherListResponse>>(data),
            Skip = skip,
            Take = take,
            Total = totalCount
        };
    }

    public async Task<GetPaymentVoucherResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PaymentVouchersByIdSpecification(new PaymentVoucherId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetPaymentVoucherResponse>(item);
    }

    public async Task<GetPaymentVoucherResponse> GetAsync(long voucherNumber, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PaymentVouchersByNumberSpecification(voucherNumber))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetPaymentVoucherResponse>(item);
    }

    public async Task CreateAsync(PaymentVoucherRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var paymentVoucher = PaymentVoucher.Create(
            request.Amount,
            request.VoucherNumber,
            request.Description,
            request.ExchangeRate,
            request.PaymentDate,
            new FinancialAccountId(request.SourceFinancialAccountId),
            new FinancialAccountId(request.DestinationFinancialAccountId),
            new PriceUnitId(request.VoucherPriceUnitId)
        );

        await repository.CreateAsync(paymentVoucher, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, PaymentVoucherRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var item = await repository
            .Get(new PaymentVouchersByIdSpecification(new PaymentVoucherId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetAmount(request.Amount);
        item.SetVoucherNumber(request.VoucherNumber);
        item.SetDescription(request.Description);
        item.SetPaymentDate(request.PaymentDate);
        item.SetSourceFinancialAccountId(new FinancialAccountId(request.SourceFinancialAccountId));
        item.SetDestinationFinancialAccountId(new FinancialAccountId(request.DestinationFinancialAccountId));
        item.SetVoucherPriceUnitId(new PriceUnitId(request.VoucherPriceUnitId));
        item.SetExchangeRate(request.ExchangeRate);

        await repository.UpdateAsync(item, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PaymentVouchersByIdSpecification(new PaymentVoucherId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await deleteValidator.ValidateAndThrowAsync(item, cancellationToken);

        await repository.DeleteAsync(item, cancellationToken);
    }

    public async Task<GetVoucherNumberResponse> GetLastNumberAsync(CancellationToken cancellationToken = default)
    {
        var lastNumber = await repository.GetLastNumberAsync(cancellationToken);
        return new GetVoucherNumberResponse(lastNumber);
    }
}