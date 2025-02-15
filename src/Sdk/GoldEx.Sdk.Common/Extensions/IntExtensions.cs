namespace GoldEx.Sdk.Common.Extensions;

public static class IntExtensions
{
    public static int GenerateRandomNumber()
    {
        var random = new Random();
        return random.Next(100000, 999999);
    }
}