using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.StoreAggregate;

public readonly record struct StoreId(Guid Value);

public class Store : EntityBase<StoreId>
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? LogoUrl { get; private set; }
    public string? BackgroundImageUrl { get; private set; }
    public string? CustomDomain { get; private set; }
    public bool IsActive { get; private set; }

    public static Store Create(
        string name,
        string slug,
        string? logoUrl = null,
        string? backgroundImageUrl = null,
        string? customDomain = null)
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
            CustomDomain = NormalizeDomain(customDomain),
            IsActive = true
        };
    }

    public static Store CreateDefaultStore(
        string name = "فروشگاه مرکزی",
        string slug = "default",
        string? customDomain = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Store
        {
            Id = new StoreId(Guid.Empty),
            Name = name,
            Slug = slug.ToLowerInvariant().Trim(),
            CustomDomain = NormalizeDomain(customDomain),
            IsActive = true
        };
    }

#pragma warning disable CS8618
    private Store() { }
#pragma warning restore CS8618

    public void UpdateDetails(
        string name,
        string slug,
        string? logoUrl,
        string? backgroundImageUrl,
        string? customDomain = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        Name = name;
        Slug = slug.ToLowerInvariant().Trim();
        LogoUrl = logoUrl;
        BackgroundImageUrl = backgroundImageUrl;
        CustomDomain = NormalizeDomain(customDomain);
    }

    public void SetCustomDomain(string? customDomain)
    {
        CustomDomain = NormalizeDomain(customDomain);
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    private static string? NormalizeDomain(string? domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return null;

        var clean = domain.Trim().ToLowerInvariant().TrimEnd('/');
        if (clean.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            clean = clean["https://".Length..];
        else if (clean.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            clean = clean["http://".Length..];

        return clean.TrimEnd('/');
    }
}
