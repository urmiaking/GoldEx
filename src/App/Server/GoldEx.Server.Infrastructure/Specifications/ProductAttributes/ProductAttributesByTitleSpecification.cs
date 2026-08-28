using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAttributeAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.ProductAttributes;

public class ProductAttributesByTitleSpecification : SpecificationBase<ProductAttribute>
{
    public ProductAttributesByTitleSpecification(string title, ProductAttributeId? excludeId = null)
    {
        if (excludeId.HasValue)
        {
            AddCriteria(x => x.Title == title.Trim() && x.Id != excludeId.Value);
        }
        else
        {
            AddCriteria(x => x.Title == title.Trim());
        }
    }
}
