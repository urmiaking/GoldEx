using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using System.Globalization;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Routings;

public class ApiUrls
{
    private static string BuildUrl(params string[] segments)
    {
        return string.Join('/', segments);
    }

    private static CultureInfo EnCulture => new("en-US")
    {
        DateTimeFormat =
        {
            DateSeparator = "-"
        }
    };

    public class Icons
    {
        public static string Get(IconType iconType, Guid id) =>
            BuildUrl(ApiRoutes.Icons.Base, ApiRoutes.Icons.GetIcon).FormatRoute(new { iconType, id })
                .AppendQueryString(iconType is IconType.App ? new { DateTime.Now } : null);
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
        public static string Get() => BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.Get);

        public static string Get(MarketType marketType) =>
            BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.GetMarket).FormatRoute(new { marketType });

        public static string Get(GoldUnitType unitType, Guid? priceUnitId, bool? applySafetyMargin = true) =>
            BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.GetUnit).FormatRoute(new { unitType, priceUnitId }).AppendQueryString(new { applySafetyMargin });

        public static string GetByPriceUnit(Guid priceUnitId) =>
            BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.GetByPriceUnit).FormatRoute(new { priceUnitId });

        public static string GetSettings() => BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.GetSettings);

        public static string GetTitles(MarketType[] marketTypes) => BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.GetTitles)
            .AppendQueryString(new { marketTypes });

        public static string UpdateStatus(Guid id) =>
            BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.UpdateStatus).FormatRoute(new { id });

        public static string GetExchangeRate(Guid primaryPriceUnitId, Guid secondaryPriceUnitId) =>
            BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.GetExchange)
                .FormatRoute(new { primaryPriceUnitId, secondaryPriceUnitId });
    }

    public class Health
    {
        public static string Get() => BuildUrl(ApiRoutes.Health.Base);
    }

    public class Products
    {
        public static string GetList(RequestFilter filter, ProductFilter productFilter) => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.GetList)
            .AppendQueryString(filter).AppendQueryString(productFilter);

        public static string GetList(string name) =>
            BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.GetListByName).AppendQueryString(new { name });

        public static string Get(Guid id) => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.Get).FormatRoute(new { id });

        public static string Get(string barcode, bool? forCalculation = true) 
            => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.GetByBarcode)
                .FormatRoute(new { barcode }).AppendQueryString(new { forCalculation });

        public static string Create() => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.Create);

        public static string Update(Guid id) =>
            BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.Update).FormatRoute(new { id });

        public static string Delete(Guid id) =>
            BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.Delete).FormatRoute(new { id });
    }

    public class Settings
    {
        public static string Get() => BuildUrl(ApiRoutes.Settings.Base, ApiRoutes.Settings.Get);
        public static string Update() => BuildUrl(ApiRoutes.Settings.Base, ApiRoutes.Settings.Update);
    }

    public class ProductCategories
    {
        public static string GetList() => BuildUrl(ApiRoutes.ProductCategories.Base, ApiRoutes.ProductCategories.GetList);

        public static string Get(Guid id) => BuildUrl(ApiRoutes.ProductCategories.Base, ApiRoutes.ProductCategories.Get)
            .FormatRoute(new { id });

        public static string Create() => BuildUrl(ApiRoutes.ProductCategories.Base, ApiRoutes.ProductCategories.Create);

        public static string Update(Guid id) =>
            BuildUrl(ApiRoutes.ProductCategories.Base, ApiRoutes.ProductCategories.Update).FormatRoute(new { id });

        public static string Delete(Guid id) =>
            BuildUrl(ApiRoutes.ProductCategories.Base, ApiRoutes.ProductCategories.Delete).FormatRoute(new { id });
    }

    public class Customers
    {
        public static string GetList(RequestFilter filter, CustomerFilter customerFilter) =>
            BuildUrl(ApiRoutes.Customers.Base, ApiRoutes.Customers.GetList)
                .AppendQueryString(filter)
                .AppendQueryString(customerFilter);
        public static string Get(Guid id) =>
            BuildUrl(ApiRoutes.Customers.Base, ApiRoutes.Customers.Get).FormatRoute(new { id });
        public static string GetByNationalId(string nationalId) =>
            BuildUrl(ApiRoutes.Customers.Base, ApiRoutes.Customers.GetByNationalId)
            .FormatRoute(new { nationalId });
        public static string GetByPhoneNumber(string phoneNumber) =>
            BuildUrl(ApiRoutes.Customers.Base, ApiRoutes.Customers.GetByPhoneNumber)
                .FormatRoute(new { phoneNumber });
        public static string Create() => BuildUrl(ApiRoutes.Customers.Base, ApiRoutes.Customers.Create);
        public static string Update(Guid id) =>
            BuildUrl(ApiRoutes.Customers.Base, ApiRoutes.Customers.Update).FormatRoute(new { id });
        public static string Delete(Guid id) =>
            BuildUrl(ApiRoutes.Customers.Base, ApiRoutes.Customers.Delete).FormatRoute(new { id });
    }

    public class Transactions
    {
        public static string GetList(RequestFilter filter, Guid? customerId) =>
            BuildUrl(ApiRoutes.Transactions.Base, ApiRoutes.Transactions.GetList).AppendQueryString(filter).AppendQueryString(new { customerId });

        public static string Get(Guid id) =>
            BuildUrl(ApiRoutes.Transactions.Base, ApiRoutes.Transactions.Get).FormatRoute(new { id });

        public static string Get(int number) =>
            BuildUrl(ApiRoutes.Transactions.Base, ApiRoutes.Transactions.GetByNumber)
                .AppendQueryString(new { number });

        public static string Create() =>
            BuildUrl(ApiRoutes.Transactions.Base, ApiRoutes.Transactions.Create);

        public static string Update(Guid id) =>
            BuildUrl(ApiRoutes.Transactions.Base, ApiRoutes.Transactions.Update).FormatRoute(new { id });

        public static string Delete(Guid id) =>
            BuildUrl(ApiRoutes.Transactions.Base, ApiRoutes.Transactions.Delete).FormatRoute(new { id });

        public static string GetLastNumber() =>
            BuildUrl(ApiRoutes.Transactions.Base, ApiRoutes.Transactions.GetLastNumber);
    }

    public class PriceUnits
    {
        public static string GetList() => BuildUrl(ApiRoutes.PriceUnits.Base, ApiRoutes.PriceUnits.GetList);

        public static string GetAll() => BuildUrl(ApiRoutes.PriceUnits.Base, ApiRoutes.PriceUnits.GetAll);

        public static string GetTitles() => BuildUrl(ApiRoutes.PriceUnits.Base, ApiRoutes.PriceUnits.GetTitles);

        public static string Get(Guid id) => BuildUrl(ApiRoutes.PriceUnits.Base, ApiRoutes.PriceUnits.Get)
            .FormatRoute(new { id });

        public static string GetDefault() => BuildUrl(ApiRoutes.PriceUnits.Base, ApiRoutes.PriceUnits.GetDefault);

        public static string Create() => BuildUrl(ApiRoutes.PriceUnits.Base, ApiRoutes.PriceUnits.Create);

        public static string Update(Guid id) =>
            BuildUrl(ApiRoutes.PriceUnits.Base, ApiRoutes.PriceUnits.Update).FormatRoute(new { id });

        public static string SetStatus(Guid id) =>
            BuildUrl(ApiRoutes.PriceUnits.Base, ApiRoutes.PriceUnits.UpdateStatus).FormatRoute(new { id });

        public static string SetAsDefault(Guid id) =>
            BuildUrl(ApiRoutes.PriceUnits.Base, ApiRoutes.PriceUnits.SetAsDefault).FormatRoute(new { id });

    }

    public class PaymentMethods
    {
        public static string GetList() => BuildUrl(ApiRoutes.PaymentMethods.Base, ApiRoutes.PaymentMethods.GetList);
        public static string GetAll() => BuildUrl(ApiRoutes.PaymentMethods.Base, ApiRoutes.PaymentMethods.GetAll);
        public static string Get(Guid id) => BuildUrl(ApiRoutes.PaymentMethods.Base, ApiRoutes.PaymentMethods.Get)
            .FormatRoute(new { id });
        public static string Create() => BuildUrl(ApiRoutes.PaymentMethods.Base, ApiRoutes.PaymentMethods.Create);
        public static string Update(Guid id) =>
            BuildUrl(ApiRoutes.PaymentMethods.Base, ApiRoutes.PaymentMethods.Update).FormatRoute(new { id });
        public static string UpdateStatus(Guid id) =>
            BuildUrl(ApiRoutes.PaymentMethods.Base, ApiRoutes.PaymentMethods.UpdateStatus).FormatRoute(new { id });
    }

    public class Invoices
    {
        public static string Create() => BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.Create);

        public static string GetList(RequestFilter filter, InvoiceFilter invoiceFilter, Guid? customerId) =>
            BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.GetList)
                .AppendQueryString(filter)
                .AppendQueryString(invoiceFilter)
                .AppendQueryString(new { customerId });

        public static string Delete(Guid id, bool deleteProducts) =>
            BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.Delete)
                .FormatRoute(new { id })
                .AppendQueryString(new { deleteProducts });

        public static string Get(Guid id) =>
            BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.Get)
                .FormatRoute(new { id });

        public static string Get(long invoiceNumber) =>
            BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.GetByNumber)
                .FormatRoute(new { invoiceNumber });

        public static string GetLastNumber() =>
            BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.GetLastNumber);
    }

    public class Reports
    {
        public static string GetList() =>
            BuildUrl(ApiRoutes.Reports.Base, ApiRoutes.Reports.GetList);
    }
}