using GoldEx.Server.Domain.CustomerAggregate;

namespace GoldEx.Server.Domain.CoinInstanceAggregate;

public class CoinInstancePackage
{
    public decimal VacuumedWeight { get; private set; }
    public string? CardColor { get; private set; }
    public string StandardCode { get; private set; }

    public CustomerId? IssuerId { get; private set; }
    public Customer? Issuer { get; private set; }

    private CoinInstancePackage(decimal vacuumedWeight, string? cardColor, string standardCode, CustomerId? issuerId)
    {
        VacuumedWeight = vacuumedWeight;
        CardColor = cardColor;
        StandardCode = standardCode;
        IssuerId = issuerId;
    }

#pragma warning disable CS8618 
    private CoinInstancePackage() { }
#pragma warning restore CS8618

    public static CoinInstancePackage Create(decimal vacuumedWeight, string? cardColor, string standardCode, CustomerId? issuerId)
    {
        return new CoinInstancePackage(vacuumedWeight, cardColor, standardCode, issuerId);
    }
}