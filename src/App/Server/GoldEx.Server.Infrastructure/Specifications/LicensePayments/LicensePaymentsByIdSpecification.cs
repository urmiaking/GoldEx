using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.LicensePaymentAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.LicensePayments;

public class LicensePaymentsByIdSpecification(LicensePaymentId id) : SpecificationBase<LicensePayment>(x => x.Id == id);