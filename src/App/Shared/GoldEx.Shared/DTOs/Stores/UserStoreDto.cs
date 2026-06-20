using System;

namespace GoldEx.Shared.DTOs.Stores;

public class UserStoreDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? LogoUrl { get; set; }
    public string? BackgroundImageUrl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsCurrent { get; set; }
}
