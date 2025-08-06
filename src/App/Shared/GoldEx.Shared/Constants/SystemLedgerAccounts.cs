namespace GoldEx.Shared.Constants;

public static class SystemLedgerAccounts
{
    #region Top-Level Accounts (سرفصل‌های اصلی)
    public static string Assets => "دارایی‌ها";
    public static string Liabilities => "بدهی‌ها";
    public static string Equity => "حقوق صاحبان سهام";
    public static string Revenue => "درآمدها";
    public static string Expenses => "هزینه‌ها";
    #endregion

    #region Asset Sub-Accounts (زیرمجموعه دارایی‌ها)
    public static string CurrentAssets => "دارایی‌های جاری";
    public static string AccountsReceivable => "حساب‌های دریافتنی";
    public static string PrepaymentsToSuppliers => "پیش‌پرداخت به تامین‌کنندگان";
    public static string Inventory => "موجودی کالا";
    public static string Banks => "بانک‌ها";
    public static string CashAccounts => "صندوق‌ها";
    #endregion

    #region Liability Sub-Accounts (زیرمجموعه بدهی‌ها)
    public static string CurrentLiabilities => "بدهی‌های جاری";
    public static string AccountsPayable => "حساب‌های پرداختنی";
    #endregion

    #region Revenue Sub-Accounts (زیرمجموعه درآمدها)
    public static string SalesRevenue => "درآمد فروش";
    public static string ExchangeGainLoss => "سود و زیان تسعیر ارز";
    #endregion

    #region Expense Sub-Accounts (زیرمجموعه هزینه‌ها)
    public static string CostOfGoodsSold => "بهای تمام شده کالای فروش رفته";
    public static string OperatingExpenses => "هزینه‌های عملیاتی";
    #endregion
}