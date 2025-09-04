using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryStocks;

public class InventoryStocksByCalculatorFilterSpecification : SpecificationBase<InventoryStock>
{
    public InventoryStocksByCalculatorFilterSpecification(CalculatorFilterRequest filter)
    {
        AddInclude(x => x.Product!);

        AddCriteria(item => item.Product!.ProductType == filter.ProductType);

        AddCriteria(item => item.Product!.GoldUnitType == filter.UnitType);

        if (filter.Fineness.HasValue)
        {
            AddCriteria(x => x.Product!.Fineness == filter.Fineness);
        }

        if (filter.ProductCategoryId.HasValue)
        {
            AddCriteria(item => item.Product!.ProductCategoryId.HasValue &&
                                item.Product.ProductCategoryId.Value.Value == filter.ProductCategoryId.Value);
        }

        if (filter.MinWeight.HasValue)
        {
            AddCriteria(item => item.Product!.Weight >= filter.MinWeight.Value);
        }

        if (filter.MaxWeight.HasValue)
        {
            AddCriteria(item => item.Product!.Weight <= filter.MaxWeight.Value);
        }

        if (filter.MaxWage.HasValue)
        {
            AddCriteria(item => item.Product!.WageType == WageType.Percent &&
                                item.Product.Wage <= filter.MaxWage.Value);
        }
    }
}