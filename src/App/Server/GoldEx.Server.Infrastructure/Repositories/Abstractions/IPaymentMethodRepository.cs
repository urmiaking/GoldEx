using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PaymentMethodAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IPaymentMethodRepository : IRepository<PaymentMethod>,
    ICreateRepository<PaymentMethod>,
    IUpdateRepository<PaymentMethod>,
    IDeleteRepository<PaymentMethod>;