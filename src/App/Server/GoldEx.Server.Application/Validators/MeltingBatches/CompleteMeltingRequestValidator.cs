using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.MeltingBatches;
using GoldEx.Shared.DTOs.MeltingBatches;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.MeltingBatches;

[ScopedService]
internal class CompleteMeltingRequestValidator : AbstractValidator<(Guid MeltingBatchId, CompleteMeltingRequestDto Request)>
{
    private readonly IMeltingBatchRepository _meltingBatchRepository;
    public CompleteMeltingRequestValidator(IMeltingBatchRepository meltingBatchRepository)
    {
        _meltingBatchRepository = meltingBatchRepository;
        RuleFor(x => x.MeltingBatchId)
            .MustAsync(BeInValidState)
            .WithMessage("وضعیت درخواست ذوب معتبر نمی باشد.");

        RuleFor(x => x.Request.AssayNumber)
            .NotEmpty().WithMessage("شماره انگ نمی‌تواند خالی باشد.");

        RuleFor(x => x.Request.Fineness)
            .NotEmpty().WithMessage("عیار نمی‌تواند خالی باشد.");

        RuleFor(x => x.Request.Weight)
            .NotEmpty().WithMessage("وزن نمی‌تواند خالی باشد.");
    }

    private async Task<bool> BeInValidState(Guid meltingBatchId, CancellationToken cancellationToken)
    {
        var meltingBatch = await _meltingBatchRepository
            .Get(new MeltingBatchesByIdSpecification(new MeltingBatchId(meltingBatchId)))
            .FirstOrDefaultAsync(cancellationToken);

        return meltingBatch?.CurrentStatus is MeltingBatchStatus.SentToLab;
    }
}