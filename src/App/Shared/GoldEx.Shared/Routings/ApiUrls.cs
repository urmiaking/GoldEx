using GoldEx.Sdk.Common.Extensions;

namespace GoldEx.Shared.Routings;

public class ApiUrls
{
    private static string BuildUrl(params string[] segments)
    {
        return string.Join('/', segments);
    }

    public static class Account
    {
        public static string LinkExternalLogin() => BuildUrl(ApiRoutes.Account.Base, ApiRoutes.Account.Manage.Base, ApiRoutes.Account.Manage.LinkExternalLogin);

        public static string Logout(string? returnUrl) => BuildUrl(ApiRoutes.Account.Base, ApiRoutes.Account.Logout)
            .AppendQueryString(new { returnUrl });

        public static string PerformExternalLogin() =>
            BuildUrl(ApiRoutes.Account.Base, ApiRoutes.Account.PerformExternalLogin);
    }

    public class Price
    {
        public static string GetLatestPrices() => BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.GetLatestPrices);
    }

    public class Health
    {
        public static string Get() => BuildUrl(ApiRoutes.Health.Base);
    }

    public class Images
    {
        public static string GetImage(string imageUrl) => BuildUrl(ApiRoutes.Images.Base, ApiRoutes.Images.GetImage)
            .AppendQueryString(new { imageUrl });
    }

    public class Products
    {
        public static string GetList() => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.GetList);
        public static string Get(Guid id) => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.Get).FormatRoute(new { id });
        public static string GetByBarcode(string barcode) => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.GetByBarcode)
            .AppendQueryString(new { barcode });

        public static string Create() => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.Create);
        public static string Update() => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.Update);
        public static string Delete(Guid id) => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.Delete).FormatRoute(new { id });

    }
}