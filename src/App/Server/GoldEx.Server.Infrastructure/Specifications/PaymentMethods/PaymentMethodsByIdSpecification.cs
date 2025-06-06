using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PaymentMethodAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PaymentMethods;

public class PaymentMethodsByIdSpecification : SpecificationBase<PaymentMethod>
{
    public PaymentMethodsByIdSpecification(PaymentMethodId id)
    {
        AddCriteria(x => x.Id == id);
    }
}