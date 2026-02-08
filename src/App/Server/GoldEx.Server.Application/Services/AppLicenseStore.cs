using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.AppLicenseAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.AppLicenses;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class AppLicenseStore(IAppLicenseRepository repository) : ILicenseStore
{
    public Task<AppLicense?> GetAsync(CancellationToken cancellationToken = default)
    {
        return repository.Get(new AppLicensesDefaultSpecification()).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SetAsync(AppLicense newLicense, CancellationToken cancellationToken = default)
    {
        var existingLicense = await GetAsync(cancellationToken);

        if (existingLicense is null)
        {
            await repository.CreateAsync(newLicense, cancellationToken);
            return;
        }

        // Same License → safe update
        if (existingLicense.LicenseId == newLicense.LicenseId)
        {
            existingLicense.UpdateVerificationKey(newLicense.VerificationKey);
            await repository.UpdateAsync(existingLicense, cancellationToken);
            return;
        }

        // License changed → recreate
        await repository.DeleteAsync(existingLicense, cancellationToken);
        await repository.CreateAsync(newLicense, cancellationToken);
    }

    public async Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        var appLicense = await GetAsync(cancellationToken);

        if (appLicense is not null) 
            await repository.DeleteAsync(appLicense, cancellationToken);
    }
}