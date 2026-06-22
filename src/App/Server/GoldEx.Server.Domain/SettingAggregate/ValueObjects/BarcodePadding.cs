using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.SettingAggregate.ValueObjects;

public sealed class BarcodePadding : EntityBase
{
    public double Top { get; private set; }
    public double Right { get; private set; }
    public double Bottom { get; private set; }
    public double Left { get; private set; }

    private BarcodePadding() { }

    private BarcodePadding(double top, double right, double bottom, double left)
    {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }

    public static BarcodePadding Create(double top, double right, double bottom, double left)
    {
        if (top < 0 || right < 0 || bottom < 0 || left < 0)
            throw new ArgumentException("Paddings cannot be negative");

        return new BarcodePadding(top, right, bottom, left);
    }

    public static BarcodePadding Default() => new(1.0, 1.0, 1.0, 1.0);
}