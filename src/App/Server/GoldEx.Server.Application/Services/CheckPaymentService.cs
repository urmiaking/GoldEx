using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.CheckPaymentAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.CheckPayments;
using GoldEx.Shared.DTOs.CheckPayments;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class CheckPaymentService(
    ICheckPaymentRepository repository,
    IMapper mapper,
    IWebHostEnvironment environment,
    IAccountingTransactionService accountingTransactionService) : ICheckPaymentService
{
    public async Task<PagedList<GetCheckPaymentListResponse>> GetListAsync(
        RequestFilter filter,
        CheckPaymentFilter checkPaymentFilter,
        CancellationToken cancellationToken = default)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        var spec = new CheckPaymentsByFilterSpecification(filter, checkPaymentFilter);

        var data = await repository
            .Get(spec)
            .ToListAsync(cancellationToken);

        var totalCount = await repository.CountAsync(spec, cancellationToken);

        var list = mapper.Map<List<GetCheckPaymentListResponse>>(data);

        // Populate ImageUrls if the files exist on disk
        var imageDirectoryPath = environment.GetCheckPaymentDirectoryPath();
        
        if (Directory.Exists(imageDirectoryPath))
        {
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                var invoicePaymentId = item.InvoicePaymentId;
                
                var filePath = Directory.GetFiles(imageDirectoryPath, invoicePaymentId + ".*")
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(filePath))
                {
                    var fileName = Path.GetFileName(filePath);
                    list[i] = item with { ImageUrl = $"/uploads/check-payments/{fileName}" };
                }
            }
        }

        return new PagedList<GetCheckPaymentListResponse>
        {
            Data = list,
            Skip = skip,
            Take = take,
            Total = totalCount
        };
    }

    public async Task AcceptAsync(Guid checkPaymentId, AcceptCheckPaymentRequest request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            var checkPayment = await repository
                .Get(new CheckPaymentsByIdSpecification(new CheckPaymentId(checkPaymentId)))
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Check payment with ID {checkPaymentId} not found.");

            if (checkPayment.CurrentStatus != CheckPaymentStatus.Pending)
                throw new InvalidOperationException("Only pending check payments can be accepted.");

            // 1. Update domain status to Accepted
            checkPayment.Accept(request.Description, new FinancialAccountId(request.TargetFinancialAccountId));
            await repository.UpdateAsync(checkPayment, cancellationToken);

            // 2. Generate double-entry transactions
            await accountingTransactionService.CreateTransactionsForCheckAcceptAsync(checkPayment, request.TargetFinancialAccountId, request.Description, cancellationToken);

            await repository.SaveAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ReturnAsync(Guid checkPaymentId, ReturnCheckPaymentRequest request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            var checkPayment = await repository
                .Get(new CheckPaymentsByIdSpecification(new CheckPaymentId(checkPaymentId)))
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"Check payment with ID {checkPaymentId} not found.");

            if (checkPayment.CurrentStatus != CheckPaymentStatus.Pending)
                throw new InvalidOperationException("Only pending check payments can be returned.");

            // 1. Update domain status to Returned
            checkPayment.Return(request.Description);
            await repository.UpdateAsync(checkPayment, cancellationToken);

            // 2. Generate double-entry transactions (reversal)
            await accountingTransactionService.CreateTransactionsForCheckReturnAsync(checkPayment, request.Description, cancellationToken);

            await repository.SaveAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
