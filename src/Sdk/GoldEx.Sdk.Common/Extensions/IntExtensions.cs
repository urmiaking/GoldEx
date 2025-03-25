namespace GoldEx.Sdk.Common.Extensions;

public static class IntExtensions
{
    public static int GenerateRandomNumber()
    {
        return Random.Shared.Next(100000, 999999);
    }

    public static int GenerateRandomNumber(int min, int max)
    {
        return Random.Shared.Next(min, max);
    }
}