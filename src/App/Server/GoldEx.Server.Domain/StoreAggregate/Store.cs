using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.StoreAggregate;

public readonly record struct StoreId(Guid Value);

public class Store : EntityBase<StoreId>
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? LogoUrl { get; private set; }
    public string? BackgroundImageUrl { get; private set; }
    public bool IsActive { get; private set; }

    public static Store Create(string name, string slug, string? logoUrl = null, string? backgroundImageUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Store
        {
            Id = new StoreId(Guid.CreateVersion7()),
            Name = name,
            Slug = slug.ToLowerInvariant().Trim(),
            LogoUrl = logoUrl,
            BackgroundImageUrl = backgroundImageUrl,
            IsActive = true
        };
    }

    public static Store CreateDefaultStore(string name = "فروشگاه مرکزی", string slug = "default")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Store
        {
            Id = new StoreId(Guid.Empty),
            Name = name,
            Slug = slug.ToLowerInvariant().Trim(),
            IsActive = true
        };
    }

#pragma warning disable CS8618
    private Store() { }
#pragma warning restore CS8618

    public void UpdateDetails(string name, string slug, string? logoUrl, string? backgroundImageUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        Name = name;
        Slug = slug.ToLowerInvariant().Trim();
        LogoUrl = logoUrl;
        BackgroundImageUrl = backgroundImageUrl;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
