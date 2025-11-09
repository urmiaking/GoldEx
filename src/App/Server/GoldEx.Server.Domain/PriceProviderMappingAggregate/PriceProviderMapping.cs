using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.PriceProviderMappingAggregate;

public readonly record struct PriceProviderMappingId(Guid Value);

public sealed class PriceProviderMapping : EntityBase<PriceProviderMappingId>
{
    public static PriceProviderMapping Create(PriceId priceId, PriceProviderType providerType, string providerSymbol, bool isEnabled = true)
    {
        return new PriceProviderMapping
        {
            Id = new PriceProviderMappingId(Guid.NewGuid()),
            PriceId = priceId,
            ProviderType = providerType,
            ProviderSymbol = providerSymbol,
            IsEnabled = isEnabled
        };
    }

#pragma warning disable CS8618
    private PriceProviderMapping() { }
#pragma warning restore CS8618

    public PriceProviderType ProviderType { get; private set; }
    public string ProviderSymbol { get; private set; }
    public bool IsEnabled { get; private set; }

    public PriceId PriceId { get; private set; }
    public Price? Price { get; private set; }

    public void SetProvider(PriceProviderType providerType, string providerSymbol)
    {
        ProviderType = providerType;
        ProviderSymbol = providerSymbol;
    }

    public void SetEnabled(bool enabled) => IsEnabled = enabled;
}