using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using System.Globalization;

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

        public static string GetPendings(DateTime checkpointDate) => BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.GetPendings)
            .FormatRoute(new { checkpointDate = checkpointDate.ToString("o", EnCulture) });
    }

    public class Health
    {
        public static string Get() => BuildUrl(ApiRoutes.Health.Base);
    }

    public class Products
    {
        public static string GetList(RequestFilter filter) => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.GetList).AppendQueryString(filter);
        public static string Get(Guid id) => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.Get).FormatRoute(new { id });
        public static string GetByBarcode(string barcode) => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.GetByBarcode)
            .AppendQueryString(new { barcode });

        public static string Create() => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.Create);

        public static string Update(Guid id) =>
            BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.Update).FormatRoute(new { id });

        public static string Delete(Guid id) =>
            BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.Delete).FormatRoute(new { id });
        public static string GetPendingItems(DateTime checkpointDate) =>
            BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.GetPendingItems)
                .FormatRoute(new { checkpointDate = checkpointDate.ToString("o", EnCulture) });
    }

    public class Settings
    {
        public static string Get() => BuildUrl(ApiRoutes.Settings.Base, ApiRoutes.Settings.GetAll);
        public static string Get(Guid id) => BuildUrl(ApiRoutes.Settings.Base, ApiRoutes.Settings.Get).FormatRoute(new { id });
        public static string Update(Guid id) => BuildUrl(ApiRoutes.Settings.Base, ApiRoutes.Settings.Update).FormatRoute(new { id });
        public static string GetUpdate(DateTime checkpointDate) =>
            BuildUrl(ApiRoutes.Settings.Base, ApiRoutes.Settings.GetUpdate)
                .FormatRoute(new { checkpointDate = checkpointDate.ToString("o", EnCulture) });
    }

    private static CultureInfo EnCulture => new("en-US")
    {
        DateTimeFormat =
        {
            DateSeparator = "-"
        }
    };
}