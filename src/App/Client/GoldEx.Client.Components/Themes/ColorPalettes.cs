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

    private static MudTheme CreateBlueTheme()
    {
        var theme = CreateBaseTheme();
        theme.PaletteLight = new PaletteLight
        {
            Primary = "#1E88E5",         
            Secondary = "#64B5F6",        
            Background = "#F7FAFF",       
            Surface = "#F3F8FE",          
            AppbarBackground = "#E3F2FD", 
            AppbarText = "#0D1B2A",
            DrawerBackground = "#F1F6FC", 
            TextPrimary = "#0D1B2A",      
            TextSecondary = "#1E88E5",    
            TextDisabled = "#1E88E580",   
            Divider = "#B0C9E6",          
            Info = "#42A5F5",             
            Success = "#43A047",          
            Warning = "#FFB300",          
            Error = "#E53935"             
        };

        theme.PaletteDark = new PaletteDark
        {
            Primary = "#1976D2",           
            Secondary = "#1565C0",         
            Surface = "#1E1E1E",
            Background = "#121212",
            BackgroundGray = "#2A2A2A",

            AppbarText = "#E3F2FD",        
            AppbarBackground = "#1E1E1E",
            DrawerBackground = "#121212",

            ActionDefault = "#1976D2",
            ActionDisabled = "#1976D280",
            ActionDisabledBackground = "#1E1E1E80",

            TextPrimary = "#E3F2FD",       
            TextSecondary = "#64B5F6",     
            TextDisabled = "#64B5F680",

            DrawerIcon = "#E3F2FD",
            DrawerText = "#E3F2FD",

            GrayLight = "#4A4F50",
            GrayLighter = "#2A2A2A",

            Info = "#4A86FF",
            Success = "#3DCB6C",
            Warning = "#FFB74D",
            Error = "#EF5350",

            Divider = "#1976D2",
            OverlayLight = "#12121280"
        };

        return theme;
    }

    private static MudTheme CreateGreenTheme()
    {
        var theme = CreateBaseTheme();
        theme.PaletteLight = new PaletteLight
        {
            Primary = "#388E3C",
            Secondary = "#81C784",
            Background = "#F7FBF8",
            Surface = "#F3F8F4",
            AppbarBackground = "#C8E6C9",
            AppbarText = "#1C1C1C",        
            DrawerBackground = "#F5FAF6",
            TextPrimary = "#1C1C1C",       
            TextSecondary = "#2E7D32",     
            TextDisabled = "#2E7D3280",
            Divider = "#A5D6A7",
            Info = "#4A86FF",
            Success = "#43A047",
            Warning = "#FBC02D",
            Error = "#E53935"
        };

        theme.PaletteDark = new PaletteDark
        {
            Primary = "#43A047",            
            Secondary = "#2E7D32",          
            Surface = "#1E1E1E",
            Background = "#121212",
            BackgroundGray = "#2A2A2A",

            AppbarText = "#E8F5E9",         
            AppbarBackground = "#1E1E1E",
            DrawerBackground = "#121212",

            ActionDefault = "#43A047",
            ActionDisabled = "#43A04780",
            ActionDisabledBackground = "#1E1E1E80",

            TextPrimary = "#E8F5E9",       
            TextSecondary = "#66BB6A",  
            TextDisabled = "#66BB6A80",

            DrawerIcon = "#E8F5E9",
            DrawerText = "#E8F5E9",

            GrayLight = "#4A4F50",
            GrayLighter = "#2A2A2A",

            Info = "#4A86FF",
            Success = "#3DCB6C",
            Warning = "#FDD835",
            Error = "#EF5350",

            Divider = "#43A047",
            OverlayLight = "#12121280"
        };


        return theme;
    }

    private static MudTheme CreatePurpleTheme()
    {
        var theme = CreateBaseTheme();
        theme.PaletteLight = new PaletteLight
        {
            Primary = "#8E24AA",
            Secondary = "#BA68C8",
            Background = "#F9F6FB",
            Surface = "#F5F0FA",
            AppbarBackground = "#E1BEE7",
            AppbarText = "#1C1C1C",        
            DrawerBackground = "#F6F2FA",
            TextPrimary = "#1C1C1C",       
            TextSecondary = "#6A1B9A",     
            TextDisabled = "#6A1B9A80",
            Divider = "#C7A9D6",
            Info = "#4A86FF",
            Success = "#43A047",
            Warning = "#FBC02D",
            Error = "#E53935"
        };

        theme.PaletteDark = new PaletteDark
        {
            Primary = "#AB47BC",            
            Secondary = "#7B1FA2",          
            Surface = "#1E1E1E",
            Background = "#121212",
            BackgroundGray = "#2A2A2A",

            AppbarText = "#E1BEE7",         
            AppbarBackground = "#1E1E1E",
            DrawerBackground = "#121212",

            ActionDefault = "#AB47BC",
            ActionDisabled = "#AB47BC80",
            ActionDisabledBackground = "#1E1E1E80",

            TextPrimary = "#F3E5F5",        
            TextSecondary = "#AB47BC",      
            TextDisabled = "#AB47BC80",

            DrawerIcon = "#F3E5F5",
            DrawerText = "#F3E5F5",

            GrayLight = "#4A4F50",
            GrayLighter = "#2A2A2A",

            Info = "#4A86FF",
            Success = "#3DCB6C",
            Warning = "#FBC02D",
            Error = "#EF5350",

            Divider = "#AB47BC",
            OverlayLight = "#12121280"
        };


        return theme;
    }

    private static MudTheme CreateRedTheme()
    {
        var theme = CreateBaseTheme();
        theme.PaletteLight = new PaletteLight
        {
            Primary = "#D32F2F",           
            Secondary = "#FF8A80",         
            Background = "#FFF6F6",        
            Surface = "#FFF9F8",           
            AppbarBackground = "#FFCDD2",  
            AppbarText = "#1C1C1C",        
            DrawerBackground = "#FFF9F8",
            TextPrimary = "#1C1C1C",       
            TextSecondary = "#C62828",     
            TextDisabled = "#C6282880",    
            Divider = "#E57373",           
            Info = "#4A86FF",
            Success = "#43A047",
            Warning = "#FBC02D",
            Error = "#E53935"
        };
        theme.PaletteDark = new PaletteDark
        {
            Primary = "#D32F2F",            
            Secondary = "#B71C1C",          
            Surface = "#1E1E1E",
            Background = "#121212",
            BackgroundGray = "#2A2A2A",

            AppbarText = "#FFCDD2",         
            AppbarBackground = "#1E1E1E",
            DrawerBackground = "#121212",

            ActionDefault = "#D32F2F",
            ActionDisabled = "#D32F2F80",
            ActionDisabledBackground = "#1E1E1E80",

            TextPrimary = "#FFEBEE",        
            TextSecondary = "#EF5350",      
            TextDisabled = "#EF535080",

            DrawerIcon = "#FFEBEE",
            DrawerText = "#FFEBEE",

            GrayLight = "#4A4F50",
            GrayLighter = "#2A2A2A",

            Info = "#4A86FF",
            Success = "#3DCB6C",
            Warning = "#FFB74D",
            Error = "#EF5350",

            Divider = "#D32F2F",
            OverlayLight = "#12121280"
        };

        return theme;
    }

    private static MudTheme CreateOrangeTheme()
    {
        var theme = CreateBaseTheme();
        theme.PaletteLight = new PaletteLight
        {
            Primary = "#F57C00",           
            Secondary = "#FFB74D",         
            Background = "#FFF8F2",        
            Surface = "#FFF9F4",           
            AppbarBackground = "#FFE0B2",  
            AppbarText = "#1C1C1C",        
            DrawerBackground = "#FFF9F4",
            TextPrimary = "#1C1C1C",       
            TextSecondary = "#E65100",     
            TextDisabled = "#E6510080",
            Divider = "#FFB74D",           
            Info = "#4A86FF",
            Success = "#43A047",
            Warning = "#FBC02D",
            Error = "#E53935"
        };

        theme.PaletteDark = new PaletteDark
        {
            Primary = "#FB8C00",           
            Secondary = "#EF6C00",         
            Surface = "#1E1E1E",
            Background = "#121212",
            BackgroundGray = "#2A2A2A",

            AppbarText = "#FFE0B2",        
            AppbarBackground = "#1E1E1E",
            DrawerBackground = "#121212",

            ActionDefault = "#FB8C00",
            ActionDisabled = "#FB8C0080",
            ActionDisabledBackground = "#1E1E1E80",

            TextPrimary = "#FFF3E0",       
            TextSecondary = "#FB8C00",     
            TextDisabled = "#FB8C0080",

            DrawerIcon = "#FFF3E0",
            DrawerText = "#FFF3E0",

            GrayLight = "#4A4F50",
            GrayLighter = "#2A2A2A",

            Info = "#4A86FF",
            Success = "#3DCB6C",
            Warning = "#FFB74D",
            Error = "#EF5350",

            Divider = "#FB8C00",
            OverlayLight = "#12121280"
        };

        return theme;
    }
}