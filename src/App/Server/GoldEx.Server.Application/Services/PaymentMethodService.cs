using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.PaymentMethods;
using GoldEx.Server.Domain.PaymentMethodAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PaymentMethods;
using GoldEx.Shared.DTOs.PaymentMethods;
using GoldEx.Shared.Services;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class PaymentMethodService(
    IPaymentMethodRepository repository,
    IMapper mapper,
    CreatePaymentMethodRequestValidator createValidator,
    UpdatePaymentMethodRequestValidator updateValidator) : IPaymentMethodService
{
    public async Task<List<GetPaymentMethodResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await repository.Get(new PaymentMethodsDefaultSpecification()).ToListAsync(cancellationToken);
        return mapper.Map<List<GetPaymentMethodResponse>>(list);
    }

    public async Task<List<GetPaymentMethodResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await repository.Get(new PaymentMethodsWithoutSpecification()).ToListAsync(cancellationToken);
        return mapper.Map<List<GetPaymentMethodResponse>>(list);
    }

    public async Task<GetPaymentMethodResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PaymentMethodsByIdSpecification(new PaymentMethodId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetPaymentMethodResponse>(item);
    }

    public async Task CreateAsync(CreatePaymentMethodRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var item = PaymentMethod.Create(request.Title);

        await repository.CreateAsync(item, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdatePaymentMethodRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync((id, request), cancellationToken);
        var item = await repository
            .Get(new PaymentMethodsByIdSpecification(new PaymentMethodId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetTitle(request.Title);

        await repository.UpdateAsync(item, cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid id, UpdatePaymentMethodStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PaymentMethodsByIdSpecification(new PaymentMethodId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetStatus(request.IsActive);

        await repository.UpdateAsync(item, cancellationToken);
    }
}