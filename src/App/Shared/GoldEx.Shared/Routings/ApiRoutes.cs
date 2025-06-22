namespace GoldEx.Shared.Routings;

public static class ApiRoutes
{
    public static class Account
    {
        public const string Base = "/api/Account";

        public const string Login = "Login";
        public const string Logout = "Logout";
        public const string PerformExternalLogin = "PerformExternalLogin";

        public static class Manage
        {
            public const string Base = "Manage";

            public const string LinkExternalLogin = "LinkExternalLogin";

        }
    }

    public class Icons
    {
        public const string Base = "/api/icons";
        public const string GetIcon = "{iconType}/{id}";
    }

    public class Price
    {
        public const string Base = "/api/Price";

        public const string Get = "";
        public const string GetMarket = "Market/{marketType}";
        public const string GetUnit = "Unit/{unitType}/{priceUnitId?}";
        public const string GetSettings = "settings";
        public const string UpdateStatus = "status/{id}";
        public const string GetTitles = "titles";
        public const string GetByPriceUnit = "price-unit/{priceUnitId}";
        public const string GetExchange = "exchange-rate/{primaryPriceUnitId}/{secondaryPriceUnitId}";
    }

    public class Health
    {
        public const string Base = "/api/Health";
    }

    public class Products
    {
        public const string Base = "/api/Products";
        public const string GetList = "";
        public const string Get = "{id}";
        public const string GetByBarcode = "barcode/{barcode}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string Delete = "{id}";
    }

    public class Settings
    {
        public const string Base = "/api/Settings";
        public const string Get = "";
        public const string Update = "";
    }

    public class ProductCategories
    {
        public const string Base = "/api/ProductCategories";
        public const string GetList = "";
        public const string Get = "{id}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string Delete = "{id}";
    }

    public class Customers
    {
        public const string Base = "/api/Customers";
        public const string GetList = "";
        public const string Get = "{id}";
        public const string GetByNationalId = "national-id/{nationalId}";
        public const string GetByPhoneNumber = "phone-number/{phoneNumber}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string Delete = "{id}";
    }

    public class Transactions
    {
        public const string Base = "/api/Transactions";
        public const string GetList = ""; // GET /api/Transactions or /api/Transactions?customerId=123
        public const string Get = "{id}";
        public const string GetByNumber = "number/{number}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string Delete = "{id}";
        public const string GetLastNumber = "last-number";
    }

    public class PriceUnits
    {
        public const string Base = "/api/PriceUnits";
        public const string GetList = "";
        public const string GetAll = "all";
        public const string GetTitles = "titles";
        public const string Get = "{id}";
        public const string GetDefault = "default";
        public const string Create = "";
        public const string Update = "{id}";
        public const string UpdateStatus = "status/{id}";
        public const string SetAsDefault = "default/{id}";
    }

    public class PaymentMethods
    {
        public const string Base = "/api/PaymentMethods";
        public const string GetList = "";
        public const string GetAll = "all";
        public const string Get = "{id}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string UpdateStatus = "status/{id}";
    }

    public class Invoices
    {
        public const string Base = "/api/Invoices";
        public const string Create = "";
        public const string Update = "{id}";
        public const string GetList = ""; // GET /api/Invoices or /api/Invoices?customerId=123
        public const string Get = "{id}";
        public const string Delete = "{id}";
        public const string GetLastNumber = "last-number";
    }
}