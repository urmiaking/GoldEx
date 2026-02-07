using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.LicensePaymentAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal sealed class LicensePaymentRepository(GoldExDbContext dbContext) : RepositoryBase<LicensePayment>(dbContext), ILicensePaymentRepository;