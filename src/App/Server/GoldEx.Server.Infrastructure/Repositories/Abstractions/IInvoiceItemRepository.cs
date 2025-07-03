using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InvoiceItemAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IInvoiceItemRepository : IRepository<InvoiceItem>,
    ICreateRepository<InvoiceItem>,
    IUpdateRepository<InvoiceItem>,
    IDeleteRepository<InvoiceItem>;