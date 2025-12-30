using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using System.Globalization;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.MeltingBatches;

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
        public static string Get(bool? isPinned) => BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.Get).AppendQueryString(new { isPinned });

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

        public static string SetPinned(Guid id, bool isPinned) =>
            BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.SetPinned).FormatRoute(new { id, isPinned });

        public static string UpdateSetting(Guid id) =>
            BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.UpdateSetting).FormatRoute(new { id });

        public static string ProviderCatalog(PriceProviderType providerType, MarketType? marketType) =>
            BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.ProviderCatalog)
                .AppendQueryString(new { providerType, marketType });

        public static string ProviderValidate(Guid priceId, PriceProviderType providerType, string providerSymbol) =>
            BuildUrl(ApiRoutes.Price.Base, ApiRoutes.Price.ProviderValidate)
                .FormatRoute(new { priceId, providerType, providerSymbol });
    }

    public class Health
    {
        public static string Get() => BuildUrl(ApiRoutes.Health.Base);
    }

    public class Products
    {
        public static string GetList(RequestFilter filter, ProductFilter productFilter) => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.GetList)
            .AppendQueryString(filter).AppendQueryString(productFilter);

        public static string GetList(string name, ProductType productType) =>
            BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.GetListByName)
                .FormatRoute(new { productType })
                .AppendQueryString(new { name });

        public static string Get(string barcode)
            => BuildUrl(ApiRoutes.Products.Base, ApiRoutes.Products.GetByBarcode)
                .FormatRoute(new { barcode });

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

        public static string GetBarcodePrintSettings() => BuildUrl(ApiRoutes.Settings.Base, ApiRoutes.Settings.GetBarcodePrintSettings);
        public static string UpdateBarcodePrintSettings() => BuildUrl(ApiRoutes.Settings.Base, ApiRoutes.Settings.UpdateBarcodePrintSettings);
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

        public static string GetLastCode() =>
            BuildUrl(ApiRoutes.ProductCategories.Base, ApiRoutes.ProductCategories.GetLastCode);
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
        public static string GetByName(string? customerName, CustomerType? type) =>
            BuildUrl(ApiRoutes.Customers.Base, ApiRoutes.Customers.GetByName)
                .FormatRoute(new { customerName })
                .AppendQueryString(new { type });

        public static string GetNames(string? name, CustomerType type) =>
            BuildUrl(ApiRoutes.Customers.Base, ApiRoutes.Customers.GetNames)
                .FormatRoute(new { type })
                .AppendQueryString(new { name });
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

    public class Invoices
    {
        public static string GetList(RequestFilter filter, InvoiceFilter invoiceFilter, Guid? customerId) =>
            BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.GetList)
                .AppendQueryString(filter)
                .AppendQueryString(invoiceFilter)
                .AppendQueryString(new { customerId });

        public static string Delete(Guid id) =>
            BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.Delete)
                .FormatRoute(new { id });

        public static string Get(Guid id) =>
            BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.Get)
                .FormatRoute(new { id });

        public static string Get(long invoiceNumber, InvoiceType invoiceType) =>
            BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.GetByNumber)
                .FormatRoute(new { invoiceNumber, invoiceType });

        public static string GetLastNumber(InvoiceType invoiceType) =>
            BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.GetLastNumber).FormatRoute(new { invoiceType });

        public static string Create() => BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.Create);

        public static string Update(Guid id) => BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.Update).FormatRoute(new { id });

        public static string SendReminder(Guid id) => BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.SendReminder).FormatRoute(new { id });

        public static string DownloadPdf(Guid id) =>
            BuildUrl(ApiRoutes.Invoices.Base, ApiRoutes.Invoices.DownloadPdf).FormatRoute(new { id });
    }

    public class Reports
    {
        public static string GetList() =>
            BuildUrl(ApiRoutes.Reports.Base, ApiRoutes.Reports.GetList);
    }

    public class FinancialAccounts
    {
        public static string GetAll() =>
            BuildUrl(ApiRoutes.FinancialAccounts.Base, ApiRoutes.FinancialAccounts.GetAll);
        public static string GetList(RequestFilter filter, FinancialAccountFilter financialAccountFilter) =>
            BuildUrl(ApiRoutes.FinancialAccounts.Base, ApiRoutes.FinancialAccounts.GetList)
                .AppendQueryString(filter)
                .AppendQueryString(financialAccountFilter);
        public static string Get(Guid id) =>
            BuildUrl(ApiRoutes.FinancialAccounts.Base, ApiRoutes.FinancialAccounts.Get).FormatRoute(new { id });
        public static string Create() => BuildUrl(ApiRoutes.FinancialAccounts.Base, ApiRoutes.FinancialAccounts.Create);
        public static string Update(Guid id) =>
            BuildUrl(ApiRoutes.FinancialAccounts.Base, ApiRoutes.FinancialAccounts.Update).FormatRoute(new { id });
        public static string Delete(Guid id) =>
            BuildUrl(ApiRoutes.FinancialAccounts.Base, ApiRoutes.FinancialAccounts.Delete).FormatRoute(new { id });
        public static string GetTitles(Guid? customerId, Guid? priceUnitId) =>
            BuildUrl(ApiRoutes.FinancialAccounts.Base, ApiRoutes.FinancialAccounts.GetTitles)
                .AppendQueryString(new { customerId })
                .AppendQueryString(new { priceUnitId });
    }

    public class PaymentVouchers
    {
        public static string GetList(RequestFilter filter, PaymentVoucherFilter voucherFilter, Guid? customerId) =>
            BuildUrl(ApiRoutes.PaymentVouchers.Base, ApiRoutes.PaymentVouchers.GetList)
                .AppendQueryString(filter)
                .AppendQueryString(voucherFilter)
                .AppendQueryString(new { customerId });
        public static string Get(Guid id) =>
            BuildUrl(ApiRoutes.PaymentVouchers.Base, ApiRoutes.PaymentVouchers.Get).FormatRoute(new { id });
        public static string Get(long voucherNumber) =>
            BuildUrl(ApiRoutes.PaymentVouchers.Base, ApiRoutes.PaymentVouchers.GetByNumber)
                .AppendQueryString(new { voucherNumber });
        public static string Create() => BuildUrl(ApiRoutes.PaymentVouchers.Base, ApiRoutes.PaymentVouchers.Create);
        public static string Update(Guid id) =>
            BuildUrl(ApiRoutes.PaymentVouchers.Base, ApiRoutes.PaymentVouchers.Update).FormatRoute(new { id });
        public static string Delete(Guid id) =>
            BuildUrl(ApiRoutes.PaymentVouchers.Base, ApiRoutes.PaymentVouchers.Delete).FormatRoute(new { id });
        public static string GetLastNumber() =>
            BuildUrl(ApiRoutes.PaymentVouchers.Base, ApiRoutes.PaymentVouchers.GetLastNumber);
        public static string GetByNumber(long voucherNumber) =>
            BuildUrl(ApiRoutes.PaymentVouchers.Base, ApiRoutes.PaymentVouchers.GetByNumber)
                .FormatRoute(new { voucherNumber });
        public static string GetPendingList(Guid customerId) =>
            BuildUrl(ApiRoutes.PaymentVouchers.Base, ApiRoutes.PaymentVouchers.GetPendingList).FormatRoute(new { customerId });
    }

    public class LedgerAccounts
    {
        public static string GetList(Guid? customerId) =>
            BuildUrl(ApiRoutes.LedgerAccounts.Base, ApiRoutes.LedgerAccounts.GetList)
                .AppendQueryString(new { customerId });
        public static string Get(Guid id) =>
            BuildUrl(ApiRoutes.LedgerAccounts.Base, ApiRoutes.LedgerAccounts.Get).FormatRoute(new { id });
        public static string Create() => BuildUrl(ApiRoutes.LedgerAccounts.Base, ApiRoutes.LedgerAccounts.Create);
        public static string Update(Guid id) =>
            BuildUrl(ApiRoutes.LedgerAccounts.Base, ApiRoutes.LedgerAccounts.Update).FormatRoute(new { id });
        public static string Delete(Guid id) =>
            BuildUrl(ApiRoutes.LedgerAccounts.Base, ApiRoutes.LedgerAccounts.Delete).FormatRoute(new { id });

        public static string GetTitles(FinancialAccountType? financialAccountType) =>
            BuildUrl(ApiRoutes.LedgerAccounts.Base, ApiRoutes.LedgerAccounts.GetTitles)
                .AppendQueryString(new { financialAccountType });
    }

    public class Coins
    {
        public static string GetList(bool? isActive) =>
            BuildUrl(ApiRoutes.Coins.Base, ApiRoutes.Coins.GetList).AppendQueryString(new { isActive });

        public static string Get(Guid id) =>
            BuildUrl(ApiRoutes.Coins.Base, ApiRoutes.Coins.Get).FormatRoute(new { id });

        public static string Create() => BuildUrl(ApiRoutes.Coins.Base, ApiRoutes.Coins.Create);

        public static string Update(Guid id) =>
            BuildUrl(ApiRoutes.Coins.Base, ApiRoutes.Coins.Update).FormatRoute(new { id });

        public static string SetStatus(Guid id, bool isActive) =>
            BuildUrl(ApiRoutes.Coins.Base, ApiRoutes.Coins.SetStatus).FormatRoute(new { id, isActive });

        public static string GetPrice(Guid coinId, Guid? priceUnitId) =>
            BuildUrl(ApiRoutes.Coins.Base, ApiRoutes.Coins.GetPrice).FormatRoute(new { coinId, priceUnitId });

        public static string Delete(Guid id) =>
            BuildUrl(ApiRoutes.Coins.Base, ApiRoutes.Coins.Delete).FormatRoute(new { id });
    }

    public class InventoryStocks
    {
        public static string GetList(RequestFilter filter, InventoryFilter inventoryFilter) =>
            BuildUrl(ApiRoutes.InventoryStocks.Base, ApiRoutes.InventoryStocks.GetList)
                .AppendQueryString(filter)
                .AppendQueryString(inventoryFilter);

        public static string GetAvailableProducts(CalculatorFilterRequest calculatorFilter, RequestFilter filter) =>
            BuildUrl(ApiRoutes.InventoryStocks.Base, ApiRoutes.InventoryStocks.GetAvailableProducts)
                .AppendQueryString(calculatorFilter)
                .AppendQueryString(filter);

        public static string GetInventoryWeightChart(WarehouseActionType actionType) =>
            BuildUrl(ApiRoutes.InventoryStocks.Base, ApiRoutes.InventoryStocks.GetInventoryWeightChart)
                .FormatRoute(new { actionType });

        public static string GetInvoiceInventoryItems(Guid invoiceId, RequestFilter requestFilter) =>
            BuildUrl(ApiRoutes.InventoryStocks.Base, ApiRoutes.InventoryStocks.GetInvoiceInventoryItems)
                .FormatRoute(new { invoiceId })
                .AppendQueryString(requestFilter);

        public static string GetTraces(Guid itemId, ItemType itemType, RequestFilter requestFilter) =>
            BuildUrl(ApiRoutes.InventoryStocks.Base, ApiRoutes.InventoryStocks.GetTraces)
                .FormatRoute(new { itemId, itemType })
                .AppendQueryString(requestFilter);

        public static string GetAvailableItemAmount(Guid itemId, ItemType itemType) =>
            BuildUrl(ApiRoutes.InventoryStocks.Base, ApiRoutes.InventoryStocks.GetAvailableItemAmount)
                .FormatRoute(new { itemId, itemType });

        public static string DeleteProduct(Guid productId) =>
            BuildUrl(ApiRoutes.InventoryStocks.Base, ApiRoutes.InventoryStocks.DeleteProduct)
                .FormatRoute(new { productId });
    }

    public class Transactions
    {
        public static string GetRemainingList(Guid customerId, Guid? priceUnitId) =>
            BuildUrl(ApiRoutes.Transactions.Base, ApiRoutes.Transactions.GetRemainingList)
                .FormatRoute(new { customerId })
                .AppendQueryString(new { priceUnitId });

        public static string GetList(TransactionFilter transactionFilter, RequestFilter requestFilter) =>
            BuildUrl(ApiRoutes.Transactions.Base, ApiRoutes.Transactions.GetList)
                .AppendQueryString(transactionFilter)
                .AppendQueryString(requestFilter);

        public static string GetFinancialAccountBalance(Guid financialAccountId) =>
            BuildUrl(ApiRoutes.Transactions.Base, ApiRoutes.Transactions.GetFinancialAccountBalance)
                .FormatRoute(new { financialAccountId });

        public static string GetAccountBalance() =>
            BuildUrl(ApiRoutes.Transactions.Base, ApiRoutes.Transactions.GetAccountBalance);

        public static string GetAvailablePriceUnits(TransactionFilter transactionFilter) =>
            BuildUrl(ApiRoutes.Transactions.Base, ApiRoutes.Transactions.GetAvailablePriceUnits)
                .AppendQueryString(transactionFilter);
    }

    public class Notifications
    {
        public static string GetList()
            => BuildUrl(ApiRoutes.Notifications.Base, ApiRoutes.Notifications.GetList);
        public static string MarkAsRead(Guid id)
            => BuildUrl(ApiRoutes.Notifications.Base, ApiRoutes.Notifications.MarkAsRead).FormatRoute(new { id });
        public static string MarkAllAsRead()
            => BuildUrl(ApiRoutes.Notifications.Base, ApiRoutes.Notifications.MarkAllAsRead);
    }

    public class MeltingBatches
    {
        public static string GetList(RequestFilter filter, MeltingBatchFilter meltingBatchFilter) =>
            BuildUrl(ApiRoutes.MeltingBatches.Base, ApiRoutes.MeltingBatches.GetList)
                .AppendQueryString(filter)
                .AppendQueryString(meltingBatchFilter);

        public static string Get(Guid id) =>
            BuildUrl(ApiRoutes.MeltingBatches.Base, ApiRoutes.MeltingBatches.Get).FormatRoute(new { id });

        public static string Create() => BuildUrl(ApiRoutes.MeltingBatches.Base, ApiRoutes.MeltingBatches.Create);

        public static string SendToLab(Guid id) =>
            BuildUrl(ApiRoutes.MeltingBatches.Base, ApiRoutes.MeltingBatches.SendToLab).FormatRoute(new { id });

        public static string CompleteMelting(Guid id) =>
            BuildUrl(ApiRoutes.MeltingBatches.Base, ApiRoutes.MeltingBatches.CompleteMelting).FormatRoute(new { id });
    }

    public class BarcodeInquiries
    {
        public static string GetList(string? barcode) =>
            BuildUrl(ApiRoutes.BarcodeInquiries.Base, ApiRoutes.BarcodeInquiries.GetList)
                .AppendQueryString(new { barcode });

        public static string Inquiry(string barcode) =>
            BuildUrl(ApiRoutes.BarcodeInquiries.Base, ApiRoutes.BarcodeInquiries.Inquiry)
                .FormatRoute(new { barcode });
    }

    public static class BarcodeReservations
    {
        public static string IssueNext() =>
            BuildUrl(ApiRoutes.BarcodeReservations.Base, ApiRoutes.BarcodeReservations.IssueNext);

        public static string Release(BarcodeType barcodeType, string barcode) =>
            BuildUrl(ApiRoutes.BarcodeReservations.Base, ApiRoutes.BarcodeReservations.Release)
                .FormatRoute(new { barcodeType, barcode });
    }

    public static class PriceProviders
    {
        public static string Validate(Guid priceId) =>
            BuildUrl(ApiRoutes.PriceProviders.Base, ApiRoutes.PriceProviders.Validate)
                .FormatRoute(new { priceId });

        public static string GetCatalog(PriceProviderType providerType, MarketType? marketType, bool? live = null) =>
            BuildUrl(ApiRoutes.PriceProviders.Base, ApiRoutes.PriceProviders.GetCatalog)
                .AppendQueryString(new { providerType, marketType, live });

        public static string Upsert(Guid priceId) =>
            BuildUrl(ApiRoutes.PriceProviders.Base, ApiRoutes.PriceProviders.Upsert)
                .FormatRoute(new { priceId });
    }

    public class InventoryEntries
    {
        public static string Create() => BuildUrl(ApiRoutes.InventoryEntries.Base, ApiRoutes.InventoryEntries.Create);

        public static string ProcessExcel() =>
            BuildUrl(ApiRoutes.InventoryEntries.Base, ApiRoutes.InventoryEntries.ProcessExcel);

        public static string GetList(RequestFilter filter) =>
            BuildUrl(ApiRoutes.InventoryEntries.Base, ApiRoutes.InventoryEntries.GetList)
                .AppendQueryString(filter);

        public static string Rollback(Guid id) =>
            BuildUrl(ApiRoutes.InventoryEntries.Base, ApiRoutes.InventoryEntries.Rollback)
                .FormatRoute(new { id });
    }

    public class Files
    {
        public static string GetInventoryEntryTemplate() =>
            BuildUrl(ApiRoutes.Files.Base, ApiRoutes.Files.GetInventoryEntryTemplate);
    }

    public class BlogCategories
    {
        public static string GetList() => BuildUrl(ApiRoutes.BlogCategories.Base, ApiRoutes.BlogCategories.GetList);

        public static string Get(Guid id) => BuildUrl(ApiRoutes.BlogCategories.Base, ApiRoutes.BlogCategories.Get)
            .FormatRoute(new { id });

        public static string Create() => BuildUrl(ApiRoutes.BlogCategories.Base, ApiRoutes.BlogCategories.Create);

        public static string Update(Guid id) =>
            BuildUrl(ApiRoutes.BlogCategories.Base, ApiRoutes.BlogCategories.Update).FormatRoute(new { id });

        public static string SetStatus(Guid id, bool isActive) =>
            BuildUrl(ApiRoutes.BlogCategories.Base, ApiRoutes.BlogCategories.SetStatus).FormatRoute(new { id, isActive });

        public static string Delete(Guid id) =>
            BuildUrl(ApiRoutes.BlogCategories.Base, ApiRoutes.BlogCategories.Delete).FormatRoute(new { id });
    }

    public class BlogPosts
    {
        public static string Get(Guid id) => BuildUrl(ApiRoutes.BlogPosts.Base, ApiRoutes.BlogPosts.Get)
            .FormatRoute(new { id });

        public static string Get(string slug) => BuildUrl(ApiRoutes.BlogPosts.Base, ApiRoutes.BlogPosts.GetSlug)
            .FormatRoute(new { slug });

        public static string Create() => BuildUrl(ApiRoutes.BlogPosts.Base, ApiRoutes.BlogPosts.Create);

        public static string Update(Guid id) =>
            BuildUrl(ApiRoutes.BlogPosts.Base, ApiRoutes.BlogPosts.Update).FormatRoute(new { id });

        public static string SetStatus(Guid id, bool isActive) =>
            BuildUrl(ApiRoutes.BlogPosts.Base, ApiRoutes.BlogPosts.SetStatus).FormatRoute(new { id, isActive });

        public static string Delete(Guid id) =>
            BuildUrl(ApiRoutes.BlogPosts.Base, ApiRoutes.BlogPosts.Delete).FormatRoute(new { id });

        public static string UploadFiles() => BuildUrl(ApiRoutes.BlogPosts.Base, ApiRoutes.BlogPosts.UploadFiles);

        public static string Exists(string slug) =>
            BuildUrl(ApiRoutes.BlogPosts.Base, ApiRoutes.BlogPosts.Exists)
                .FormatRoute(new { slug });
    }

    public class InventoryExits
    {
        public static string Exit() =>
            BuildUrl(ApiRoutes.InventoryExits.Base, ApiRoutes.InventoryExits.Exit);
    }

    public class CoinInstances
    {
        public static string Get(string barcode) =>
            BuildUrl(ApiRoutes.CoinInstances.Base, ApiRoutes.CoinInstances.Get)
                .FormatRoute(new { barcode });
    }
}