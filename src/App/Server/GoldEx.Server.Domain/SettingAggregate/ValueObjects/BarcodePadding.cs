using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.SettingAggregate.ValueObjects;

public sealed class BarcodePadding : EntityBase
{
    public int Top { get; private set; }
    public int Right { get; private set; }
    public int Bottom { get; private set; }
    public int Left { get; private set; }

    private BarcodePadding() { }

    private BarcodePadding(int top, int right, int bottom, int left)
    {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }

    public static BarcodePadding Create(int top, int right, int bottom, int left)
    {
        if (top < 0 || right < 0 || bottom < 0 || left < 0)
            throw new ArgumentException("Paddings cannot be negative");

        return new BarcodePadding(top, right, bottom, left);
    }

    public static BarcodePadding Default() => new(1, 15, 3, 20);
}