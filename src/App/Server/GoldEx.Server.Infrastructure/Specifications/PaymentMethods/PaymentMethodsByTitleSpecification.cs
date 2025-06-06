using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PaymentMethodAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PaymentMethods;

public class PaymentMethodsByTitleSpecification : SpecificationBase<PaymentMethod>
{
    public PaymentMethodsByTitleSpecification(string title)
    {
        AddCriteria(x => x.Title == title);
    }
}