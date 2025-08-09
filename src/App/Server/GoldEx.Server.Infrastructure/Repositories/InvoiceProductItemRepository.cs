using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InvoiceProductItemAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class InvoiceProductItemRepository(GoldExDbContext dbContext) : RepositoryBase<InvoiceProductItem>(dbContext), IInvoiceProductItemRepository;