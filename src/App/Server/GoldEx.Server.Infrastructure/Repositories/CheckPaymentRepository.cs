using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CheckPaymentAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal sealed class CheckPaymentRepository(GoldExDbContext dbContext) : RepositoryBase<CheckPayment>(dbContext), ICheckPaymentRepository;