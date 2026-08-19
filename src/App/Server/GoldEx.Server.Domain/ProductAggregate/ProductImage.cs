using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.ProductAggregate;

public class ProductImage : EntityBase
{
    public static ProductImage Create(string url, bool isMain = false, int displayOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        return new ProductImage
        {
            Url = url.Trim(),
            IsMain = isMain,
            DisplayOrder = displayOrder
        };
    }

#pragma warning disable CS8618
    private ProductImage() { }
#pragma warning restore CS8618

    public string Url { get; private set; }
    public bool IsMain { get; private set; }
    public int DisplayOrder { get; private set; }

    public void SetMain(bool isMain) => IsMain = isMain;
    public void SetDisplayOrder(int displayOrder) => DisplayOrder = displayOrder;
}
