using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InvoicePaymentAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IInvoicePaymentRepository : IRepository<InvoicePayment>,
    ICreateRepository<InvoicePayment>,
    IUpdateRepository<InvoicePayment>,
    IDeleteRepository<InvoicePayment>;