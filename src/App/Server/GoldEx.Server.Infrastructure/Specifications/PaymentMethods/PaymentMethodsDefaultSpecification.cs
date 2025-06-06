using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PaymentMethodAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PaymentMethods;

public class PaymentMethodsDefaultSpecification : SpecificationBase<PaymentMethod>
{
    public PaymentMethodsDefaultSpecification()
    {
        AddCriteria(x => x.IsActive);
    }
}