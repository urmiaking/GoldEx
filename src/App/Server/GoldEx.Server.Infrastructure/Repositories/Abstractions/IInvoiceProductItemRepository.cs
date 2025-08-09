using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InvoiceItemProductAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IInvoiceProductItemRepository : IRepository<InvoiceProductItem>,
    ICreateRepository<InvoiceProductItem>,
    IUpdateRepository<InvoiceProductItem>,
    IDeleteRepository<InvoiceProductItem>;