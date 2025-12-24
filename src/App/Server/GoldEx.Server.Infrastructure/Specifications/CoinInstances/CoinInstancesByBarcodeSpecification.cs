using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CoinInstanceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.CoinInstances;

public class CoinInstancesByBarcodeSpecification : SpecificationBase<CoinInstance>
{
    public CoinInstancesByBarcodeSpecification(string barcode)
    {
        AddCriteria(x => x.Barcode == barcode);
        AddInclude(x => x.Coin!);
        AddInclude(x => x.CoinInstancePackage!.Issuer!);
    }
}