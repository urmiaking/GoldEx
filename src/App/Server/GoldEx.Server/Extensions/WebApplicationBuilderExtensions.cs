using DevExpress.Security.Resources;
using GoldEx.Server.Application.Utilities;

namespace GoldEx.Server.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void SetupIconDirectory(this WebApplicationBuilder builder)
    {
        var env = builder.Environment;

        var iconPath = env.GetAppIconDirectory();
        var absoluteIconPath = Path.Combine(env.WebRootPath, iconPath);

        AppDomain.CurrentDomain.SetData("DXResourceDirectory", absoluteIconPath);
        AccessSettings.StaticResources.TrySetRules(DirectoryAccessRule.Allow(absoluteIconPath));
    }
}