using MudBlazor;

namespace GoldEx.Client.Components.Utilities;

public static class Masks
{
    public static PatternMask ShabaMask = new("IR XXXX XXXX XXXX XXXX XXXX XXXX")
    {
        MaskChars = [new MaskChar('X', @"[0-9]")],
        Placeholder = '_',
        CleanDelimiters = true,
        Transformation = AllUpperCase
    };

    public static PatternMask CreditCardMask = new("XXXX XXXX XXXX XXXX")
    {
        MaskChars = [new MaskChar('X', @"[0-9]")],
        Placeholder = '_',
        CleanDelimiters = true,
        Transformation = AllUpperCase
    };

    public static PatternMask SwiftBicCode = new("XXXX-XX-XX-XXX")
    {
        MaskChars = [new MaskChar('X', @"[A-Za-z0-9]")],
        Placeholder = '_',
        CleanDelimiters = true,
        Transformation = AllUpperCase
    };

    public static PatternMask InternationalIbanMask = new("XXXX XXXX XXXX XXXX XXXX XXXX XXXX")
    {
        MaskChars = [new MaskChar('X', @"[A-Za-z0-9]")],
        Placeholder = '_',
        CleanDelimiters = true,
        Transformation = AllUpperCase
    };

    private static char AllUpperCase(char c) => char.ToUpperInvariant(c);
}
