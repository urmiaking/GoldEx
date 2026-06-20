using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Domain.CoinAggregate;

public readonly record struct CoinId(Guid Value);
public class Coin : EntityBase<CoinId>, IStoreFiltered
{
    public StoreId StoreId { get; private set; }

    public static Coin Create(string title, decimal weight, decimal fineness, int startMintYear, int? endMintYear, LedgerAccountId ledgerAccountId, PriceId? priceId = null, StoreId storeId = default)
    {
        return new Coin
        {
            Id = new CoinId(Guid.CreateVersion7()),
            Title = title,
            Weight = weight,
            Fineness = fineness,
            StartMintYear = startMintYear,
            EndMintYear = endMintYear,
            LedgerAccountId = ledgerAccountId,
            PriceId = priceId,
            IsActive = true,
            StoreId = storeId
        };
    }

    public string Title { get; private set; }
    public decimal Weight { get; private set; }
    public decimal Fineness { get; private set; }
    public int StartMintYear { get; private set; }
    public int? EndMintYear { get; private set; }
    public bool IsActive { get; private set; }

    public PriceId? PriceId { get; private set; }
    public Price? Price { get; private set; }

    public LedgerAccountId LedgerAccountId { get; private set; }
    public LedgerAccount? LedgerAccount { get; private set; }

    public void SetTitle(string title) => Title = title;
    public void SetPriceId(PriceId? priceId) => PriceId = priceId;
    public void SetStatus(bool isActive) => IsActive = isActive;
    public void SetWeight(decimal weight) => Weight = weight;
    public void SetFineness(decimal fineness) => Fineness = fineness;
    public void SetMintYears(int startMintYear, int? endMintYear)
    {
        StartMintYear = startMintYear;
        EndMintYear = endMintYear;
    }

#pragma warning disable CS8618 
    private Coin() { }
#pragma warning restore CS8618 
}