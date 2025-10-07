using FluentValidation;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.MeltingBatches;
using GoldEx.Shared.DTOs.MeltingBatches;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.MeltingBatches;

internal class SendToLabRequestValidator : AbstractValidator<(Guid MeltingBatchId, SendToLabRequestDto Request)>
{
    private readonly IMeltingBatchRepository _meltingBatchRepository;
    private readonly ICustomerRepository _customerRepository;

    public SendToLabRequestValidator(IMeltingBatchRepository meltingBatchRepository, ICustomerRepository customerRepository)
    {
        _meltingBatchRepository = meltingBatchRepository;
        _customerRepository = customerRepository;

        RuleFor(x => x.MeltingBatchId)
            .MustAsync(BeInValidState)
            .WithMessage("وضعیت درخواست ذوب معتبر نمی باشد.");

        RuleFor(x => x.Request.AssayerId)
            .MustAsync(BeValidAssayer)
            .WithMessage("آزمایشگاه انتخاب شده معتبر نمی باشد.");

        RuleFor(x => x.Request.Description)
            .MaximumLength(200).WithMessage("توضیحات نباید بیشتر از 200 کاراکتر باشد.");
    }

    private async Task<bool> BeValidAssayer(Guid assayerId, CancellationToken cancellationToken = default)
    {
        var assayer = await _customerRepository.Get(new CustomersByIdSpecification(new CustomerId(assayerId)))
            .FirstOrDefaultAsync(cancellationToken);

        return assayer is { CustomerType: CustomerType.AssayingLab };
    }

    private async Task<bool> BeInValidState(Guid meltingBatchId, CancellationToken cancellationToken)
    {
        var meltingBatch = await _meltingBatchRepository.Get(new MeltingBatchesByIdSpecification(new MeltingBatchId(meltingBatchId)))
            .FirstOrDefaultAsync(cancellationToken);

        if (meltingBatch == null)
            return false;

        return meltingBatch.CurrentStatus == MeltingBatchStatus.Melting;
    }
}