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
        public const string Base = "/api/price";

        public const string Get = "";
        public const string GetMarket = "market/{marketType}";
        public const string GetUnit = "unit/{unitType}/{priceUnitId?}";
        public const string GetSettings = "settings";
        public const string UpdateStatus = "status/{id}";
        public const string GetTitles = "titles";
        public const string GetByPriceUnit = "price-unit/{priceUnitId}";
        public const string GetExchange = "exchange-rate/{primaryPriceUnitId}/{secondaryPriceUnitId}";
        public const string SetPinned = "{id}/pin/{isPinned}";
        public const string UpdateSetting = "setting/{id}";
        public const string ProviderCatalog = "provider/catalog";
        public const string ProviderValidate = "{priceId}/provider/validate/{providerType}/{providerSymbol}";
    }

    public class Health
    {
        public const string Base = "api/Health";
    }

    public class Products
    {
        public const string Base = "/api/products";
        public const string GetList = "";
        public const string GetByBarcode = "barcode/{barcode}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string Delete = "{id}";
        public const string GetListByName = "search/{productType}";
    }

    public class Settings
    {
        public const string Base = "/api/settings";
        public const string Get = "";
        public const string Update = "";
        public const string GetBarcodePrintSettings = "barcode-print";
        public const string UpdateBarcodePrintSettings = "barcode-print";
    }

    public class ProductCategories
    {
        public const string Base = "/api/product-categories";
        public const string GetList = "";
        public const string Get = "{id}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string Delete = "{id}";
        public const string GetLastCode = "last-code";
    }

    public class Customers
    {
        public const string Base = "/api/customers";
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
        public const string Base = "/api/transactions";
        public const string GetList = "";
        public const string GetRemainingList = "customer/{customerId}/remaining-list";
        public const string GetFinancialAccountBalance = "financial-account/{financialAccountId}/balance";
    }

    public class PriceUnits
    {
        public const string Base = "/api/price-units";
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
        public const string Base = "/api/invoices";
        public const string Create = "";
        public const string Update = "{id}";
        public const string GetList = "";
        public const string Get = "{id}";
        public const string Delete = "{id}";
        public const string GetLastNumber = "last-number/{invoiceType}";
        public const string GetByNumber = "number/{number}/{invoiceType}";
        public const string SendReminder = "{id}/send-reminder";
        public const string DownloadPdf = "{id}/download-pdf";
    }

    public class Reports
    {
        public const string Base = "/api/reports";
        public const string GetList = "";
    }

    public class FinancialAccounts
    {
        public const string Base = "/api/financial-accounts";
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
        public const string Base = "/api/payment-vouchers";
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
        public const string Base = "/api/ledger-accounts";
        public const string GetList = "";
        public const string Get = "{id}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string Delete = "{id}";
        public const string GetTitles = "titles";
    }

    public class Coins
    {
        public const string Base = "/api/coins";
        public const string GetList = "";
        public const string Get = "{id}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string Delete = "{id}";
        public const string SetStatus = "status/{id}/{isActive}";
        public const string GetPrice = "price/{coinId}/{priceUnitId?}";
    }

    public class InventoryStocks
    {
        public const string Base = "/api/inventory-stocks";
        public const string GetList = "";
        public const string GetAvailableProducts = "available";
        public const string GetInventoryWeightChart = "{actionType}/weight-chart";
        public const string GetInvoiceInventoryItems = "invoice/{invoiceId}/items";
        public const string GetTraces = "{itemType}/{itemId}/traces";
        public const string GetAvailableItemAmount = "{itemType}/{itemId}/available-amount";
        public const string DeleteProduct = "product/{productId}";
    }

    public class Notifications
    {
        public const string Base = "/api/notifications";
        public const string GetList = "";
        public const string MarkAsRead = "{id}/read";
        public const string MarkAllAsRead = "read-all";
    }

    public class MeltingBatches
    {
        public const string Base = "/api/melting-batches";
        public const string GetList = "";
        public const string Get = "{id}";
        public const string Create = "";
        public const string SendToLab = "{id}/send-to-lab";
        public const string CompleteMelting = "{id}/complete-melting";
    }

    public class BarcodeInquiries
    {
        public const string Base = "/api/barcode-inquiries";
        public const string GetList = "";
        public const string Inquiry = "{barcode}";
    }

    public class BarcodeReservations
    {
        public const string Base = "/api/barcode-reservations";
        public const string IssueNext = "issue-next";
        public const string Release = "{barcodeType}/{barcode}/release";

    }

    public class PriceProviders
    {
        public const string Base = "/api/price-providers";
        public const string GetCatalog = "catalog";
        public const string Validate = "validate/{priceId}";
        public const string Upsert = "{priceId}";
    }

    public class InventoryEntries
    {
        public const string Base = "/api/inventory-entries";
        public const string GetList = "";
        public const string Create = "";
        public const string ProcessExcel = "process-excel";
        public const string Rollback = "{id}";
    }

    public class InventoryExits
    {
        public const string Base = "/api/inventory-exits";
        public const string GetList = "";
        public const string Exit = "";
    }

    public class Files
    {
        public const string Base = "/api/files";
        public const string GetInventoryEntryTemplate = "inventory-entry-template";
    }

    public class BlogCategories
    {
        public const string Base = "/api/blog-categories";
        public const string GetList = "";
        public const string Get = "{id}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string SetStatus = "status/{id}/{isActive}";
        public const string Delete = "{id}";
    }

    public class BlogPosts
    {
        public const string Base = "/api/blog-posts";
        public const string Get = "{id}";
        public const string GetSlug = "slug/{slug}";
        public const string Create = "";
        public const string Update = "{id}";
        public const string SetStatus = "status/{id}/{isActive}";
        public const string Delete = "{id}";
        public const string UploadFiles = "upload";
    }

    public class CoinInstances
    {
        public const string Base = "/api/coin-instances";
        public const string Get = "{barcode}";
    }
}