using MudBlazor;

namespace GoldEx.Client.Components.Themes;

public static class ColorPalettes
{
    // Keep persisted keys compatible; labels describe the redesigned palette families.
    public static IReadOnlyList<(string Id, string Title)> Options { get; } =
        [("Gold", "طلایی"), ("Blue", "اقیانوس"), ("Green", "یشم"), ("Purple", "زنبق"), ("Red", "رز"), ("Orange", "کهربا")];

    public static readonly Dictionary<string, MudTheme> Palettes = new()
    {
        ["Gold"] = CreateGoldTheme(),
        ["Blue"] = Create("#174E9B", "#245FBB", "#F3F7FD", "#FFFFFF", "#FBFCFF", "#192C47", "#5B6E88", "#E0E7F2", "#78AAFF", "#8DB9FF", "#101725", "#192337", "#141E30", "#E7EEFC", "#A4B5D0", "#30415F"),
        ["Green"] = Create("#123D51", "#187F87", "#F4F8FB", "#FFFFFF", "#FFFFFF", "#172B35", "#5D707B", "#E1EAF0", "#32CBA4", "#45DAB6", "#10191E", "#19272E", "#142128", "#E8F1F1", "#A1B5BA", "#2E454D"),
        ["Purple"] = Create("#593D96", "#7051B7", "#F7F5FC", "#FFFFFF", "#FFFFFF", "#302641", "#736780", "#E8E1F1", "#B99AEF", "#C6A8FF", "#191422", "#251E30", "#1F1929", "#F0E9FA", "#BAACCB", "#493A5A"),
        ["Red"] = Create("#984353", "#A44058", "#FCF5F6", "#FFFFFF", "#FFFAFB", "#40272E", "#7D626B", "#EEDFE3", "#ED9EAF", "#F3B3C3", "#21161C", "#30212A", "#291C23", "#F8E9EE", "#CCADB8", "#553B46"),
        ["Orange"] = Create("#7D471C", "#9B5719", "#FBF7F0", "#FFFDF9", "#FFFDF9", "#392D21", "#7A6957", "#EBDFCD", "#EFB85E", "#F7C77C", "#1C1711", "#2A2219", "#241D15", "#EFE7D8", "#C3B297", "#4D4030")
    };

    private static MudTheme CreateBaseTheme()
    {
        return new MudTheme
        {
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "24px",
            },
            Typography = new Typography
            {
                Default = new DefaultTypography
                {
                    FontFamily = ["IRANSans", "Vazirmatn", "Helvetica", "Arial", "sans-serif"]
                }
            }
        };
    }

    private static MudTheme CreateGoldTheme()
    {
        var theme = CreateBaseTheme();
        theme.PaletteLight = new PaletteLight
        {
            Black = "#1C2526",
            AppbarText = "#1C2526",
            AppbarBackground = "#FAF9F6",
            DrawerBackground = "#F7F5F0",
            GrayLight = "#D3D3D3",
            GrayLighter = "#F8F8F8",
            Primary = "#C9A227",           
            Secondary = "#C9A227",
            Background = "#FAFAF8",        
            Surface = "#F9F8F5",           
            TextPrimary = "#1C2526",
            TextSecondary = "#B8860B",     
            TextDisabled = "#B8860B80",
            ActionDefault = "#C9A227",
            ActionDisabled = "#C9A22780",
            ActionDisabledBackground = "#F9F8F580",
            Info = "#4A86FF",
            Success = "#3DCB6C",
            Warning = "#EACD2F",           
            Error = "#FF3F5F",
            Divider = "#D8CBA0"            
        };
        theme.PaletteDark = new PaletteDark
        {
            Primary = "#DAA520",
            Secondary = "#E6B800",
            Surface = "#1E1E1E",
            Background = "#121212",
            BackgroundGray = "#2A2A2A",
            AppbarText = "#FDF5E6",
            AppbarBackground = "#1E1E1E",
            DrawerBackground = "#121212",
            ActionDefault = "#DAA520",
            ActionDisabled = "#DAA52080",
            ActionDisabledBackground = "#B8860B80",
            TextPrimary = "#FDF5E6",
            TextSecondary = "#E6B800",
            TextDisabled = "#E6B80080",
            DrawerIcon = "#FDF5E6",
            DrawerText = "#FDF5E6",
            GrayLight = "#4A4F50",
            GrayLighter = "#2A2A2A",
            Info = "#4a86ff",
            Success = "#3dcb6c",
            Warning = "#E6B800",
            Error = "#ff3f5f",
            Divider = "#B8860B",
            OverlayLight = "#12121280"
        };

        return theme;
    }

    private static MudTheme Create(
        string primary, string secondary, string background, string surface, string drawer,
        string text, string muted, string border, string darkPrimary, string darkSecondary,
        string darkBackground, string darkSurface, string darkDrawer, string darkText,
        string darkMuted, string darkBorder, string onPrimary = "#FFFFFF")
    {
        return new MudTheme
        {
            LayoutProperties = new LayoutProperties { DefaultBorderRadius = "16px" },
            Typography = new Typography
            {
                Default = new DefaultTypography { FontFamily = ["IRANSans", "Vazirmatn", "Helvetica", "Arial", "sans-serif"] }
            },
            PaletteLight = new PaletteLight
            {
                Primary = primary, PrimaryContrastText = onPrimary,
                Secondary = secondary, SecondaryContrastText = "#FFFFFF",
                Background = background, BackgroundGray = background, Surface = surface,
                AppbarBackground = surface, AppbarText = text, DrawerBackground = drawer,
                DrawerText = text, DrawerIcon = muted, TextPrimary = text, TextSecondary = muted,
                TextDisabled = "#94918B", ActionDefault = muted, ActionDisabled = "#94918B",
                ActionDisabledBackground = background, Divider = border, LinesDefault = border,
                LinesInputs = "#A19F99", TableLines = border, TableStriped = background, TableHover = background,
                Info = "#245FBB", InfoContrastText = "#FFFFFF",
                Success = "#14634E", SuccessContrastText = "#FFFFFF",
                Warning = "#825719", WarningContrastText = "#FFFFFF",
                Error = "#B33639", ErrorContrastText = "#FFFFFF"
            },
            PaletteDark = new PaletteDark
            {
                Primary = darkPrimary, PrimaryContrastText = "#151D23",
                Secondary = darkSecondary, SecondaryContrastText = "#151D23",
                Background = darkBackground, BackgroundGray = darkDrawer, Surface = darkSurface,
                AppbarBackground = darkSurface, AppbarText = darkText, DrawerBackground = darkDrawer,
                DrawerText = darkText, DrawerIcon = darkMuted, TextPrimary = darkText, TextSecondary = darkMuted,
                TextDisabled = "#8B8E92", ActionDefault = darkMuted, ActionDisabled = "#8B8E92",
                ActionDisabledBackground = darkDrawer, Divider = darkBorder, LinesDefault = darkBorder,
                LinesInputs = darkMuted, TableLines = darkBorder, TableStriped = darkDrawer, TableHover = darkDrawer,
                Info = "#8DB9FF", InfoContrastText = "#151D23",
                Success = "#65DFB1", SuccessContrastText = "#151D23",
                Warning = "#EED08A", WarningContrastText = "#151D23",
                Error = "#FFB0AC", ErrorContrastText = "#151D23",
                OverlayLight = "#10191E80"
            }
        };
    }
}
