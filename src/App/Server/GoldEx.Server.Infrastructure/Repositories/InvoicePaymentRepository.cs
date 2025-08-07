using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class InvoicePaymentRepository(GoldExDbContext dbContext) : RepositoryBase<InvoicePayment>(dbContext), IInvoicePaymentRepository;