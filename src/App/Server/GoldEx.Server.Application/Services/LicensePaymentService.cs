using System.Data;
using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Application.Exceptions;
using GoldEx.Sdk.Server.Application.Models;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.Licenses;
using GoldEx.Server.Domain.LicensePaymentAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.LicensePayments;
using GoldEx.Shared.DTOs.LicensePayments;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VHDLicenseManager;
using VHDLicenseManager.Responses;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class LicensePaymentService(
    ILicensePaymentRepository repository,
    ILicenseStore licenseStore,
    IMapper mapper,
    ProductLicense license,
    License licenseManager,
    LicensePaymentRequestValidator validator,
    ILogger<LicensePaymentService> logger) : ILicensePaymentService
{
    public async Task<List<LicensePaymentResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var list = await repository.Get(new LicensePaymentsDefaultSpecification()).ToListAsync(cancellationToken);
        return mapper.Map<List<LicensePaymentResponse>>(list);
    }

    public async Task CreateAsync(LicensePaymentRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var licensePayment = LicensePayment.Create(license.Plan, request.RequestedMonths, request.PaymentReference, request.PaymentDescription);

        await repository.CreateAsync(licensePayment, cancellationToken);
    }

    public async Task SetStatusAsync(Guid id, LicensePaymentStatus status, CancellationToken cancellationToken = default)
    {
        if (status is LicensePaymentStatus.Pending)
            throw new InvalidOperationException();

        var request = await repository
            .Get(new LicensePaymentsByIdSpecification(new LicensePaymentId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        var appLicense = await licenseStore.GetAsync(cancellationToken) ?? throw new NotFoundException("App license not found");

        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                if (status is LicensePaymentStatus.Rejected)
                {
                    request.Reject();
                    await repository.UpdateAsync(request, cancellationToken);
                }
                else
                {
                    var registerDate = DateTime.Now;
                    var expireDate = registerDate.AddMonths(request.RequestedMonths);

                    request.Approve();

                    var updated = await licenseManager.UpdateLicenseAsync(appLicense.LicenseId, LicenseType.Regular, expireDate, cancellationToken);

                    if (!updated)
                        throw new BadRequestException("خطایی در بروزرسانی لایسنس در سرور رخ داد");

                    await repository.UpdateAsync(request, cancellationToken);
                    license.UpdateLicense(LicensePlan.Regular, registerDate, expireDate);
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
}