using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CheckPaymentAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface ICheckPaymentRepository : IRepository<CheckPayment>,
    ICreateRepository<CheckPayment>,
    IUpdateRepository<CheckPayment>,
    IDeleteRepository<CheckPayment>;