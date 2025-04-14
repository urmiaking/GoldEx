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

    public class Price
    {
        public const string Base = "/api/Price";

        public const string GetLatestPrices = "GetLatestPrices";
        public const string GetPendings = "GetPendings/{checkpointDate}";
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
        public const string GetPendingItems = "pending-items/{checkPointDate}";
    }

    public class Settings
    {
        public const string Base = "/api/Settings";
        public const string GetAll = "";
        public const string Get = "{id}";
        public const string Update = "{id}";
        public const string GetUpdate = "get-update/{checkPointDate}";
    }

    public class ProductCategories
    {
        public const string Base = "/api/ProductCategories";
        public const string GetAll = "";
        public const string Get = "{id}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string Delete = "{id}";
        public const string GetPendingItems = "pending-items/{checkPointDate}";
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
        public const string GetPendingItems = "pending-items/{checkPointDate}";
    }

    public class Transactions
    {
        public const string Base = "/api/Transactions";
        public const string GetList = "";
        public const string Get = "{id}";
        public const string GetByNumber = "number/{number}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string Delete = "{id}";
        public const string GetPendingItems = "pending-items/{checkPointDate}";
        public const string GetLatestTransactionNumber = "get-latest-transaction-number";
    }
}