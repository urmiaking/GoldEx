using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Domain.CoinAggregate;

public readonly record struct CoinId(Guid Value);
public class Coin : EntityBase<CoinId>
{
    public static Coin Create(string title, LedgerAccountId ledgerAccountId, PriceId? priceId = null)
    {
        return new Coin
        {
            Id = new CoinId(Guid.NewGuid()),
            Title = title,
            LedgerAccountId = ledgerAccountId,
            PriceId = priceId,
            IsActive = true
        };
    }

    public string Title { get; private set; }
    public bool IsActive { get; private set; }

    public PriceId? PriceId { get; private set; }
    public Price? Price { get; private set; }

    public LedgerAccountId LedgerAccountId { get; private set; }
    public LedgerAccount? LedgerAccount { get; private set; }

    public void SetTitle(string title) => Title = title;
    public void SetPriceId(PriceId? priceId) => PriceId = priceId;
    public void SetStatus(bool isActive) => IsActive = isActive;

#pragma warning disable CS8618 
    private Coin() { }
#pragma warning restore CS8618 
}