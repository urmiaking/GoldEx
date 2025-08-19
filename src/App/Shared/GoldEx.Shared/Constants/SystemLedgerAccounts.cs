namespace GoldEx.Shared.Constants;

public static class SystemLedgerAccounts
{
    #region Assets (دارایی‌ها)
    public static string Assets => "دارایی‌ها";
    public static string CurrentAssets => "دارایی‌های جاری";
    public static string AccountsReceivable => "حساب‌های دریافتنی";
    public static string PrepaymentsToSuppliers => "پیش‌پرداخت به تامین‌کنندگان";
    public static string Inventory => "موجودی کالا";
    public static string CoinInventory => "موجودی سکه";
    public static string Banks => "بانک‌ها";
    public static string CashAccounts => "صندوق‌ها";
    #endregion

    #region Liabilities (بدهی‌ها)
    public static string Liabilities => "بدهی‌ها";
    public static string CurrentLiabilities => "بدهی‌های جاری";
    public static string AccountsPayable => "حساب‌های پرداختنی";
    #endregion

    #region Equity (حقوق صاحبان سهام)
    public static string Equity => "حقوق صاحبان سهام";
    public static string OpeningBalanceEquity => "سرمایه افتتاحیه - تعدیلات";
    #endregion

    #region Revenue (درآمدها)
    public static string Revenue => "درآمدها";
    public static string SalesRevenue => "درآمد فروش";
    public static string AdditionalChargesRevenue => "درآمد هزینه‌های اضافی"; // << جدید
    public static string ExchangeGainLoss => "سود و زیان تسعیر ارز";
    #endregion

    #region Expenses (هزینه‌ها)
    public static string Expenses => "هزینه‌ها";
    public static string SalesDiscounts => "تخفیفات فروش";
    public static string PurchaseDiscounts => "تخفیفات خرید";
    public static string CostOfGoodsSold => "بهای تمام شده کالای فروش رفته";
    public static string OperatingExpenses => "هزینه‌های عملیاتی";
    #endregion
}