﻿using GoldEx.Sdk.Common.Extensions;

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
}