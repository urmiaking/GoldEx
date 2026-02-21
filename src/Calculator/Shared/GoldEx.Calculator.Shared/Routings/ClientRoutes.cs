namespace GoldEx.Calculator.Shared.Routings;

public static class ClientRoutes
{
    public static class Home
    {
        public const string Index = "/";
    }

    public static class Calculator
    {
        public const string Simple = "/simple";
        public const string Currency = "/currency";
        public const string Reverse = "/reverse";
    }

    public static class About
    {
        public const string Index = "/about";
    }
}