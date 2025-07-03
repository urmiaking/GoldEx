using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PaymentMethodAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class PaymentMethodRepository(GoldExDbContext dbContext) : RepositoryBase<PaymentMethod>(dbContext), IPaymentMethodRepository;