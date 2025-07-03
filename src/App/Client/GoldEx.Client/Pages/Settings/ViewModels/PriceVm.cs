using GoldEx.Shared.DTOs.Prices;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class PriceVm : IEquatable<PriceVm>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;

    public static PriceVm CreateFrom(GetPriceTitleResponse response)
    {
        return new PriceVm
        {
            Id = response.Id,
            Title = response.Title
        };
    }

    public bool Equals(PriceVm? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Title == other.Title;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((PriceVm)obj);
    }

    public override int GetHashCode()
    {
        return Title.GetHashCode();
    }

    public override string ToString() => Title;
}