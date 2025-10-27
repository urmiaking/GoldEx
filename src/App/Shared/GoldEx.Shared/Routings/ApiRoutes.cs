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
        public const string SetPinned = "{id}/pin/{isPinned}";
    }

    public class Health
    {
        public const string Base = "api/Health";
    }

    public class Products
    {
        public const string Base = "/api/Products";
        public const string GetList = "";
        public const string GetByBarcode = "barcode/{barcode}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string Delete = "{id}";
        public const string GetListByName = "search/{productType}";
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
        public const string GetLastCode = "last-code";
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
        public const string GetByName = "name/{customerName}";
        public const string GetNames = "{type}/names";
    }

    public class Transactions
    {
        public const string Base = "/api/Transactions";
        public const string GetList = "";
        public const string GetRemainingList = "customer/{customerId}/remaining-list";
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

    public class Invoices
    {
        public const string Base = "/api/Invoices";
        public const string Create = "";
        public const string Update = "{id}";
        public const string GetList = "";
        public const string Get = "{id}";
        public const string Delete = "{id}";
        public const string GetLastNumber = "last-number/{invoiceType}";
        public const string GetByNumber = "number/{number}/{invoiceType}";
        public const string SendReminder = "{id}/send-reminder";
    }

    public class Reports
    {
        public const string Base = "/api/Reports";
        public const string GetList = "";
    }

    public class FinancialAccounts
    {
        public const string Base = "/api/FinancialAccounts";
        public const string GetAll = "";
        public const string GetList = "list";
        public const string Get = "{id}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string Delete = "{id}";
        public const string GetTitles = "titles";
    }

    public class PaymentVouchers
    {
        public const string Base = "/api/PaymentVouchers";
        public const string GetList = "";
        public const string GetPendingList = "pending/{customerId}";
        public const string Get = "{id}";
        public const string GetByNumber = "number/{voucherNumber}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string Delete = "{id}";
        public const string GetLastNumber = "last-number";
    }

    public class LedgerAccounts
    {
        public const string Base = "/api/LedgerAccounts";
        public const string GetList = "";
        public const string Get = "{id}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string Delete = "{id}";
        public const string GetTitles = "titles";
    }

    public class Coins
    {
        public const string Base = "/api/Coins";
        public const string GetList = "";
        public const string Get = "{id}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string SetStatus = "status/{id}/{isActive}";
        public const string GetPrice = "price/{coinId}/{priceUnitId?}";
    }

    public class InventoryStocks
    {
        public const string Base = "/api/InventoryStocks";
        public const string GetList = "";
        public const string GetAvailableProducts = "available";
        public const string GetInventoryWeightChart = "{targetUnit}/weight-chart";
    }

    public class Notifications
    {
        public const string Base = "/api/Notifications";
        public const string GetList = "";
        public const string MarkAsRead = "{id}/read";
        public const string MarkAllAsRead = "read-all";
    }

    public class MeltingBatches
    {
        public const string Base = "/api/MeltingBatches";
        public const string GetList = "";
        public const string Get = "{id}";
        public const string Create = "";
        public const string SendToLab = "{id}/send-to-lab";
        public const string CompleteMelting = "{id}/complete-melting";
    }
}