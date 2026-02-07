using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.LicensePaymentAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface ILicensePaymentRepository : IRepository<LicensePayment>,
    ICreateRepository<LicensePayment>,
    IUpdateRepository<LicensePayment>;