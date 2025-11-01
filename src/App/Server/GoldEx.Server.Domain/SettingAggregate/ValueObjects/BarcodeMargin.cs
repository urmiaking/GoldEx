using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.SettingAggregate.ValueObjects;

public sealed class BarcodeMargin : EntityBase
{
    public int Top { get; private set; }
    public int Right { get; private set; }
    public int Bottom { get; private set; }
    public int Left { get; private set; }

    private BarcodeMargin() { }

    private BarcodeMargin(int top, int right, int bottom, int left)
    {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }

    public static BarcodeMargin Create(int top, int right, int bottom, int left)
    {
        if (top < 0 || right < 0 || bottom < 0 || left < 0)
            throw new ArgumentException("Margins cannot be negative");

        return new BarcodeMargin(top, right, bottom, left);
    }

    public static BarcodeMargin Default() => new(5, 5, 5, 5);
}