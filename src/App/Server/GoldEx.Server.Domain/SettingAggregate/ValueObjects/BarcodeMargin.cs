using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.SettingAggregate.ValueObjects;

public sealed class BarcodeMargin : EntityBase
{
    public double Top { get; private set; }
    public double Right { get; private set; }
    public double Bottom { get; private set; }
    public double Left { get; private set; }

    private BarcodeMargin() { }

    private BarcodeMargin(double top, double right, double bottom, double left)
    {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }

    public static BarcodeMargin Create(double top, double right, double bottom, double left)
    {
        if (top < 0 || right < 0 || bottom < 0 || left < 0)
            throw new ArgumentException("Margins cannot be negative");

        return new BarcodeMargin(top, right, bottom, left);
    }

    public static BarcodeMargin Default() => new(1.0, 1.0, 1.0, 1.0);
}