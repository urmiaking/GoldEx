using System.Data;
using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Application.Exceptions;
using GoldEx.Sdk.Server.Application.Models;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.Licenses;
using GoldEx.Server.Domain.AppLicenseAggregate;
using GoldEx.Server.Domain.LicensePaymentAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.LicensePayments;
using GoldEx.Shared.DTOs.LicensePayments;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VHDLicenseManager;
using VHDLicenseManager.Responses;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class LicensePaymentService(
    ILicensePaymentRepository repository,
    IMapper mapper,
    ProductLicense license,
    License licenseManager,
    LicensePaymentRequestValidator validator,
    ILogger<LicensePaymentService> logger,
    ILicenseCache licenseCache,
    IConfiguration configuration,
    GoldExDbContext dbContext) : ILicensePaymentService
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
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

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

                    var isTenant = request.StoreId.Value != Guid.Empty;
                    var licenseMode = configuration["License:Mode"] ?? "Hybrid";

                    if (isTenant && licenseMode.Equals("Hybrid", StringComparison.OrdinalIgnoreCase))
                    {
                        var tenantLicense = await dbContext.Set<AppLicense>()
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(x => x.StoreId == request.StoreId, cancellationToken);

                        if (tenantLicense is not null)
                        {
                            tenantLicense.UpdateSubscription(LicensePlan.Regular, registerDate, expireDate);
                            await dbContext.SaveChangesAsync(cancellationToken);
                        }
                        else
                        {
                            var localLicenseId = Guid.CreateVersion7();
                            var localLicense = AppLicense.Create(
                                localLicenseId,
                                verificationKey: "LOCAL_BYPASS",
                                storeId: request.StoreId,
                                plan: LicensePlan.Regular,
                                registeredAt: registerDate,
                                expireDate: expireDate);
                            await dbContext.Set<AppLicense>().AddAsync(localLicense, cancellationToken);
                            await dbContext.SaveChangesAsync(cancellationToken);
                        }

                        await repository.UpdateAsync(request, cancellationToken);
                        license.UpdateLicense(request.StoreId.Value, LicensePlan.Regular, registerDate, expireDate);
                        licenseCache.Remove(request.StoreId.Value);
                    }
                    else
                    {
                        var masterLicense = await dbContext.Set<AppLicense>()
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(x => x.StoreId == new StoreId(Guid.Empty), cancellationToken) 
                            ?? throw new NotFoundException("Master license not found");

                        var updated = await licenseManager.UpdateLicenseAsync(masterLicense.LicenseId, LicenseType.Regular, expireDate, cancellationToken);

                        if (!updated)
                            throw new BadRequestException("خطایی در بروزرسانی لایسنس در سرور رخ داد");

                        masterLicense.UpdateSubscription(LicensePlan.Regular, registerDate, expireDate);
                        await dbContext.SaveChangesAsync(cancellationToken);

                        await repository.UpdateAsync(request, cancellationToken);
                        license.UpdateLicense(Guid.Empty, LicensePlan.Regular, registerDate, expireDate);
                        licenseCache.Remove(Guid.Empty);
                    }
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