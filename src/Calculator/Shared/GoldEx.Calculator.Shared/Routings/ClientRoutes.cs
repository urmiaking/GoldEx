namespace GoldEx.Calculator.Shared.Routings;

public static class ClientRoutes
{
    public static class Home
    {
        public const string Index = "/";
    }

    public static class Invoices
    {
        public const string Index = "/invoices";
    }

    public static class Calculator
    {
        public const string Simple = "/simple";
        public const string Currency = "/currency";
        public const string Reverse = "/reverse";
        public const string TradeIn = "/trade-in";
        public const string CoinBubble = "/coin-bubble";
        public const string Assay = "/assay";
        public const string Dca = "/dca";
    }

    public static class About
    {
        public const string Index = "/about";
    }
}