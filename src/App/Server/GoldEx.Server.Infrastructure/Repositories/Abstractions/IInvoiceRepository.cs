using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InvoiceAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IInvoiceRepository : IRepository<Invoice>,
    ICreateRepository<Invoice>,
    IUpdateRepository<Invoice>,
    IDeleteRepository<Invoice>;