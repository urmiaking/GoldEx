using MudBlazor;

namespace GoldEx.Client.Components.Themes;

public static class ColorPalettes
{
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

    public static readonly Dictionary<string, MudTheme> Palettes = new()
    {
        ["Gold"] = CreateGoldTheme(),
        ["Blue"] = CreateBlueTheme(),
        ["Green"] = CreateGreenTheme(),
        ["Purple"] = CreatePurpleTheme(),
        ["Red"] = CreateRedTheme(),
        ["Orange"] = CreateOrangeTheme()
    };

    private static MudTheme CreateGoldTheme()
    {
        var theme = CreateBaseTheme();
        theme.PaletteLight = new PaletteLight
        {
            Black = "#1C2526",
            AppbarText = "#1C2526",
            AppbarBackground = "#FDF5E6",
            DrawerBackground = "#FAF0E6",
            GrayLight = "#D3D3D3",
            GrayLighter = "#F5F5F5",

            Primary = "#DAA520",
            Secondary = "#DAA520",

            Background = "#FAF0E6",
            Surface = "#FDF5E6",

            TextPrimary = "#1C2526",
            TextSecondary = "#B8860B",
            TextDisabled = "#B8860B80",

            ActionDefault = "#DAA520",
            ActionDisabled = "#DAA52080",
            ActionDisabledBackground = "#FDF5E680",

            Info = "#4a86ff",
            Success = "#3dcb6c",
            Warning = "#FFD700",
            Error = "#ff3f5f",

            Divider = "#B8860B"
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

    private static MudTheme CreateBlueTheme()
    {
        var theme = CreateBaseTheme();
        theme.PaletteLight = new PaletteLight
        {
            Primary = "#1565c0",
            Secondary = "#4fc3f7",
            Background = "#e8f0fe",
            Surface = "#edf5fd",
            AppbarBackground = "#90caf9",
            AppbarText = "#0d1b2a",
            DrawerBackground = "#e3f2fd",
            TextPrimary = "#0d1b2a",
            TextSecondary = "#1565c0",
            TextDisabled = "#1565c080",
            Divider = "#1565c0",
            Info = "#2196f3",
            Success = "#4caf50",
            Warning = "#ff9800",
            Error = "#f44336"
        };
        theme.PaletteDark = new PaletteDark
        {
            Primary = "#64b5f6",
            Secondary = "#1e88e5",
            Background = "#1e2a44",
            Surface = "#263559",
            AppbarBackground = "#263559",
            AppbarText = "#e3f2fd",
            DrawerBackground = "#1e2a44",
            TextPrimary = "#e3f2fd",
            TextSecondary = "#bbdefb",
            TextDisabled = "#bbdefb80",
            Divider = "#64b5f6",
            Info = "#2196f3",
            Success = "#4caf50",
            Warning = "#ffb74d",
            Error = "#ef5350"
        };
        return theme;
    }

    private static MudTheme CreateGreenTheme()
    {
        var theme = CreateBaseTheme();
        theme.PaletteLight = new PaletteLight
        {
            Primary = "#2f6b31",
            Secondary = "#81c784",
            Background = "#e9f4ec",
            Surface = "#edf7ee",
            AppbarBackground = "#a5d6a7",
            AppbarText = "#1b5e20",
            DrawerBackground = "#edf7ee",
            TextPrimary = "#1b5e20",
            TextSecondary = "#388e3c",
            TextDisabled = "#388e3c80",
            Divider = "#388e3c",
            Info = "#4a86ff",
            Success = "#3dcb6c",
            Warning = "#fbc02d",
            Error = "#d32f2f"
        };
        theme.PaletteDark = new PaletteDark
        {
            Primary = "#80de88",
            Secondary = "#388e3c",
            Background = "#1a2f24",
            Surface = "#2a4a38",
            AppbarBackground = "#2a4a38",
            AppbarText = "#c8e6c9",
            DrawerBackground = "#1a2f24",
            TextPrimary = "#c8e6c9",
            TextSecondary = "#81c784",
            TextDisabled = "#81c78480",
            Divider = "#80de88",
            Info = "#4a86ff",
            Success = "#3dcb6c",
            Warning = "#fdd835",
            Error = "#ef5350"
        };
        return theme;
    }

    private static MudTheme CreatePurpleTheme()
    {
        var theme = CreateBaseTheme();
        theme.PaletteLight = new PaletteLight
        {
            Primary = "#7b1fa2",
            Secondary = "#ce93d8",
            Background = "#f3e8fa",
            Surface = "#f5e8fb",
            AppbarBackground = "#d1c4e9",
            AppbarText = "#311b92",
            DrawerBackground = "#ede7f6",
            TextPrimary = "#311b92",
            TextSecondary = "#7b1fa2",
            TextDisabled = "#7b1fa280",
            Divider = "#7b1fa2",
            Info = "#4a86ff",
            Success = "#3dcb6c",
            Warning = "#ffb300",
            Error = "#d32f2f"
        };
        theme.PaletteDark = new PaletteDark
        {
            Primary = "#ce93d8",
            Secondary = "#8e24aa",
            Background = "#23112f",
            Surface = "#3a1e4a",
            AppbarBackground = "#3a1e4a",
            AppbarText = "#e1bee7",
            DrawerBackground = "#23112f",
            TextPrimary = "#e1bee7",
            TextSecondary = "#ce93d8",
            TextDisabled = "#ce93d880",
            Divider = "#ce93d8",
            Info = "#4a86ff",
            Success = "#3dcb6c",
            Warning = "#fbc02d",
            Error = "#ef5350"
        };
        return theme;
    }

    private static MudTheme CreateRedTheme()
    {
        var theme = CreateBaseTheme();
        theme.PaletteLight = new PaletteLight
        {
            Primary = "#d32f2f",
            Secondary = "#ff6f60",
            Background = "#ffe8e8",
            Surface = "#ffefee",
            AppbarBackground = "#ffab91",
            AppbarText = "#b71c1c",
            DrawerBackground = "#ffefee",
            TextPrimary = "#b71c1c",
            TextSecondary = "#d32f2f",
            TextDisabled = "#d32f2f80",
            Divider = "#d32f2f",
            Info = "#4a86ff",
            Success = "#3dcb6c",
            Warning = "#ff9800",
            Error = "#d32f2f"
        };
        theme.PaletteDark = new PaletteDark
        {
            Primary = "#ff8a80",
            Secondary = "#e53935",
            Background = "#2a1b1b",
            Surface = "#3b1e1e",
            AppbarBackground = "#3b1e1e",
            AppbarText = "#ffcdd2",
            DrawerBackground = "#2a1b1b",
            TextPrimary = "#ffcdd2",
            TextSecondary = "#ff8a80",
            TextDisabled = "#ff8a8080",
            Divider = "#ff8a80",
            Info = "#4a86ff",
            Success = "#3dcb6c",
            Warning = "#ffb74d",
            Error = "#ef5350"
        };
        return theme;
    }

    private static MudTheme CreateOrangeTheme()
    {
        var theme = CreateBaseTheme();
        theme.PaletteLight = new PaletteLight
        {
            Primary = "#f57c00",
            Secondary = "#ffa726",
            Background = "#fff1e6",
            Surface = "#fff5e8",
            AppbarBackground = "#ffcc80",
            AppbarText = "#e65100",
            DrawerBackground = "#fff5e8",
            TextPrimary = "#e65100",
            TextSecondary = "#f57c00",
            TextDisabled = "#f57c0080",
            Divider = "#f57c00",
            Info = "#4a86ff",
            Success = "#3dcb6c",
            Warning = "#ff9800",
            Error = "#d32f2f"
        };
        theme.PaletteDark = new PaletteDark
        {
            Primary = "#ffb300",
            Secondary = "#f57c00",
            Background = "#2f1f0a",
            Surface = "#3e2a12",
            AppbarBackground = "#3e2a12",
            AppbarText = "#ffe0b2",
            DrawerBackground = "#2f1f0a",
            TextPrimary = "#ffe0b2",
            TextSecondary = "#ffb300",
            TextDisabled = "#ffb30080",
            Divider = "#ffb300",
            Info = "#4a86ff",
            Success = "#3dcb6c",
            Warning = "#ffb300",
            Error = "#ef5350"
        };
        return theme;
    }
}