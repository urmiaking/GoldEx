using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.CoinInstances;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Server.Mcp;

[ScopedService]
public class GoldExMcpEngine(
    IPriceService priceService,
    IInventoryStockService inventoryStockService,
    ICustomerService customerService,
    ITransactionService transactionService,
    IReportingService reportingService,
    IInvoiceService invoiceService,
    IPriceUnitService priceUnitService,
    IFinancialAccountService financialAccountService,
    IHttpContextAccessor httpContextAccessor,
    IStoreContext storeContext)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private string GetBaseUrl()
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return string.Empty;

        var host = context.Request.Headers["X-Forwarded-Host"].FirstOrDefault();
        if (string.IsNullOrEmpty(host))
        {
            host = context.Request.Host.Value;
        }

        var scheme = context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
        if (string.IsNullOrEmpty(scheme))
        {
            scheme = context.Request.Scheme;
        }

        if (string.IsNullOrEmpty(scheme))
        {
            scheme = "https";
        }

        return $"{scheme}://{host}";
    }

    private static DateTime? ParseDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return null;

        dateStr = dateStr.Trim().Replace('-', '/');

        // Check if Persian date format (e.g. 1404/05/01 or 1405/4/1)
        var parts = dateStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var datePart = parts[0];

        if (datePart.Contains('/'))
        {
            var segments = datePart.Split('/');
            if (segments.Length == 3 &&
                int.TryParse(segments[0], out var year) &&
                int.TryParse(segments[1], out var month) &&
                int.TryParse(segments[2], out var day))
            {
                if (year is >= 1300 and <= 1500)
                {
                    try
                    {
                        var pc = new PersianCalendar();
                        month = Math.Clamp(month, 1, 12);
                        var maxDays = pc.GetDaysInMonth(year, month);
                        day = Math.Clamp(day, 1, maxDays);
                        return pc.ToDateTime(year, month, day, 0, 0, 0, 0);
                    }
                    catch
                    {
                        // Ignore and fallback
                    }
                }
            }
        }

        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var gDate))
            return gDate;

        if (DateTime.TryParse(dateStr, out var dLocal))
            return dLocal;

        return null;
    }

    private static (DateTime start, DateTime end, string title) GetShamsiMonthRange(int monthOffset = 0)
    {
        var pc = new PersianCalendar();
        var now = DateTime.UtcNow;
        var currentYear = pc.GetYear(now);
        var currentMonth = pc.GetMonth(now);

        var targetMonth = currentMonth + monthOffset;
        var targetYear = currentYear;

        while (targetMonth < 1)
        {
            targetMonth += 12;
            targetYear -= 1;
        }
        while (targetMonth > 12)
        {
            targetMonth -= 12;
            targetYear += 1;
        }

        var start = pc.ToDateTime(targetYear, targetMonth, 1, 0, 0, 0, 0);
        var daysInMonth = pc.GetDaysInMonth(targetYear, targetMonth);
        var end = pc.ToDateTime(targetYear, targetMonth, daysInMonth, 23, 59, 59, 999);

        var monthNames = new[] { "", "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
        var title = $"{monthNames[targetMonth]} {targetYear}";

        return (start, end, title);
    }

    public List<McpToolDefinition> GetTools()
    {
        return
        [
            new McpToolDefinition
            {
                Name = "get_live_gold_prices",
                Description = "دریافت نرخ‌های لحظه‌ای بازار طلا، سکه و ارزها (طلای ۱۸ و ۲۴ عیار، مظنه، سکه امامی، بهار، نیم، ربع، گرمی و دلار)",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["isPinnedOnly"] = new() { Type = "boolean", Description = "فقط نمادهای سنجاق‌شده/مهم را برگرداند؟ (اختیاری)" }
                    }
                }
            },
            new McpToolDefinition
            {
                Name = "calculate_gold_product_price",
                Description = "محاسبه دقیق قیمت تمام‌شده طلا و جواهر شامل قیمت خام، اجرت ساخت، سود فروشنده و مالیات بر ارزش افزوده (VAT)",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["weight"] = new() { Type = "number", Description = "وزن طلا بر حسب گرم (مثلاً 4.350)" },
                        ["fineness"] = new() { Type = "number", Description = "عیار طلا (مثلاً 750 برای 18 عیار)" },
                        ["gramPrice"] = new() { Type = "number", Description = "نرخ روز هر گرم طلای 750 (بر حسب واحد پولی)" },
                        ["wageType"] = new() { Type = "string", Description = "نوع اجرت (Percent برای درصدی یا Fixed برای مبلغ ثابت)", Enum = ["Percent", "Fixed"] },
                        ["wageAmount"] = new() { Type = "number", Description = "مقدار اجرت (درصد مثلاً 12 یا مبلغ ثابت)" },
                        ["profitPercent"] = new() { Type = "number", Description = "درصد سود فروشنده (مثلاً 7)" },
                        ["taxPercent"] = new() { Type = "number", Description = "درصد مالیات بر ارزش افزوده (مثلاً 9)" },
                        ["stoneAmount"] = new() { Type = "number", Description = "ارزش سنگ/نگین در صورت وجود (اختیاری)" }
                    },
                    Required = ["weight", "fineness", "gramPrice", "profitPercent", "taxPercent"]
                }
            },
            new McpToolDefinition
            {
                Name = "calculate_scrap_gold_valuation",
                Description = "محاسبه ارزش و قیمت خرید طلای کهنه و متفرقه (دست دوم) از مشتری با احتساب کسری عیار از ۷۵۰",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["weight"] = new() { Type = "number", Description = "وزن طلای کهنه بر حسب گرم" },
                        ["fineness"] = new() { Type = "number", Description = "عیار واقعی (مثلاً 750 یا 740)" },
                        ["deductionFrom750"] = new() { Type = "number", Description = "کسری عیار از 750 (مثلاً 15 یعنی عیار موثر 735)" },
                        ["gramPrice750"] = new() { Type = "number", Description = "نرخ روز هر گرم طلای 750 (بر حسب واحد پولی)" }
                    },
                    Required = ["weight", "fineness", "deductionFrom750", "gramPrice750"]
                }
            },
            new McpToolDefinition
            {
                Name = "calculate_molten_gold",
                Description = "محاسبه وزن یا ارزش طلای آب‌شده بر اساس عیار آزمایشگاه ری‌گیری و نرخ مظنه",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["weight"] = new() { Type = "number", Description = "وزن طلای آب‌شده بر حسب گرم" },
                        ["fineness"] = new() { Type = "number", Description = "عیار ثبت‌شده از آزمایشگاه (مثلاً 785.4)" },
                        ["gramPrice"] = new() { Type = "number", Description = "نرخ روز گرم طلای 750 (بر حسب واحد پولی)" },
                        ["targetBudget"] = new() { Type = "number", Description = "در صورتی که می‌خواهید بدانید با این مبلغ چند گرم آب‌شده می‌توانید بخرید (محاسبه معکوس)" }
                    }
                }
            },
            new McpToolDefinition
            {
                Name = "search_inventory_stock",
                Description = "جستجو در موجودی انبار فروشگاه (کالاها، طلاها، جواهرات، طلای آب‌شده و سکه‌ها) با نام یا بارکد",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["query"] = new() { Type = "string", Description = "متن جستجو یا کد بارکد محصول" },
                        ["pageSize"] = new() { Type = "number", Description = "تعداد ردیف‌های خروجی (پیش‌فرض 15)" }
                    }
                }
            },
            new McpToolDefinition
            {
                Name = "get_customer_balance",
                Description = "استعلام وضعیت مانده حساب جاری مشتری به تفکیک واحدهای پولی و معادل وزنی گرم طلای ۱۸ عیار",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["customerId"] = new() { Type = "string", Description = "شناسه یکتای مشتری (در صورت وجود)" },
                        ["customerName"] = new() { Type = "string", Description = "نام یا بخشی از نام مشتری برای جستجو" }
                    }
                }
            },
            new McpToolDefinition
            {
                Name = "search_customers",
                Description = "جستجوی مشتریان و همکاران فروشگاه بر اساس نام، شماره همراه یا کد ملی",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["query"] = new() { Type = "string", Description = "نام، شماره تلفن همراه یا کد ملی مشتری" }
                    },
                    Required = ["query"]
                }
            },
            new McpToolDefinition
            {
                Name = "create_customer",
                Description = "ثبت مشتری یا همکار جدید در سیستم با نام، شماره تلفن همراه و کد ملی",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["fullName"] = new() { Type = "string", Description = "نام و نام خانوادگی مشتری (الزامی)" },
                        ["phoneNumber"] = new() { Type = "string", Description = "شماره تلفن همراه مشتری" },
                        ["nationalId"] = new() { Type = "string", Description = "کد ملی مشتری (در صورت خالی بودن خودکار تولید می‌شود)" },
                        ["address"] = new() { Type = "string", Description = "آدرس مشتری" },
                        ["customerType"] = new() { Type = "string", Description = "نوع مشتری (RetailCustomer: مشتری، Wholesaler: بنکدار، Workshop: کارگاه، Retailer: ویترین‌دار)", Enum = ["RetailCustomer", "Wholesaler", "Workshop", "Retailer"] }
                    },
                    Required = ["fullName"]
                }
            },
            new McpToolDefinition
            {
                Name = "get_customer_statement",
                Description = "دریافت صورتحساب و ریز تراکنش‌های بدهکار/بستانکار دفتر کل یک مشتری در بازه زمانی مشخص",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["customerId"] = new() { Type = "string", Description = "شناسه مشتری" },
                        ["customerName"] = new() { Type = "string", Description = "نام مشتری در صورت نداشتن شناسه" },
                        ["fromDate"] = new() { Type = "string", Description = "تاریخ شروع (شمسی مثلاً 1404/01/01 یا میلادی)" },
                        ["toDate"] = new() { Type = "string", Description = "تاریخ پایان (شمسی مثلاً 1404/05/30 یا میلادی)" }
                    }
                }
            },
            new McpToolDefinition
            {
                Name = "search_invoices",
                Description = "جستجو و فیلتر پیشرفته در فاکتورهای ثبت‌شده فروش، خرید و مرجوعی بر اساس بازه تاریخ شمسی/میلادی، نام مشتری، بارکد یا شماره فاکتور به همراه خلاصه آماری مجموع فروش و مبالغ",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["invoiceType"] = new() { Type = "string", Description = "نوع فاکتور (Sell برای فروش، Purchase برای خرید، Return برای مرجوعی)", Enum = ["Sell", "Purchase", "Return"] },
                        ["query"] = new() { Type = "string", Description = "شماره فاکتور، نام مشتری یا بارکد کالا" },
                        ["fromDate"] = new() { Type = "string", Description = "تاریخ شروع بازه (شمسی مثلاً 1404/04/01 یا میلادی)" },
                        ["toDate"] = new() { Type = "string", Description = "تاریخ پایان بازه (شمسی مثلاً 1404/05/31 یا میلادی)" },
                        ["pageSize"] = new() { Type = "number", Description = "تعداد ردیف‌های خروجی (پیش‌فرض 50، حداکثر 500)" },
                        ["page"] = new() { Type = "number", Description = "شماره صفحه (پیش‌فرض 1)" }
                    }
                }
            },
            new McpToolDefinition
            {
                Name = "get_sales_performance_report",
                Description = "گزارش جامع عملکرد و تحلیل آماری فروش فروشگاه شامل مجموع فروش، تعداد فاکتورها، میانگین مبلغ هر فاکتور، سود، اجرت، معادل وزنی طلای ۱۸ عیار فروخته‌شده و مقایسه درصدی رشد/افت با ماه قبل یا بازه گذشته",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["period"] = new() { Type = "string", Description = "نوع دوره تحلیلی (MonthOverMonth: مقایسه ماه جاری با ماه قبل، CurrentMonth: ماه جاری، PreviousMonth: ماه قبل، Custom: بازه دلخواه)", Enum = ["MonthOverMonth", "CurrentMonth", "PreviousMonth", "Custom"] },
                        ["fromDate"] = new() { Type = "string", Description = "تاریخ شروع دوره اصلی (شمسی مثلاً 1404/05/01 یا میلادی)" },
                        ["toDate"] = new() { Type = "string", Description = "تاریخ پایان دوره اصلی (شمسی مثلاً 1404/05/31 یا میلادی)" },
                        ["previousFromDate"] = new() { Type = "string", Description = "تاریخ شروع دوره مقایسه (در صورت انتخاب بازه دلخواه)" },
                        ["previousToDate"] = new() { Type = "string", Description = "تاریخ پایان دوره مقایسه (در صورت انتخاب بازه دلخواه)" }
                    }
                }
            },
            new McpToolDefinition
            {
                Name = "get_category_sales_summary",
                Description = "گزارش تجمیعی و آماری فروش به تفکیک دسته‌بندی کالا (مانند انگشتر، النگو، دستبند، سرویس، زنجیر، پلاک و...) شامل وزن واقعی طلا (گرم)، تعداد فروخته‌شده، مبلغ کل فروش، سود، اجرت و سهم درصدی از کل فروش در بازه زمانی مشخص (پاسخ مستقیم به سوالاتی مانند: مرداد چند گرم انگشتر فروختیم؟ یا از هر دسته کالا چقدر فروش داشتیم؟)",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["period"] = new() { Type = "string", Description = "نوع دوره (CurrentMonth: ماه جاری، PreviousMonth: ماه قبل، Custom: بازه دلخواه، AllTime: کل سوابق)", Enum = ["CurrentMonth", "PreviousMonth", "Custom", "AllTime"] },
                        ["fromDate"] = new() { Type = "string", Description = "تاریخ شروع بازه (شمسی مثلاً 1404/05/01 یا میلادی)" },
                        ["toDate"] = new() { Type = "string", Description = "تاریخ پایان بازه (شمسی مثلاً 1404/05/31 یا میلادی)" },
                        ["categoryName"] = new() { Type = "string", Description = "نام یا بخشی از نام دسته‌بندی کالا جهت فیلتر و استعلام اختصاصی یک دسته (مثلاً 'انگشتر' یا 'النگو')" },
                        ["categoryId"] = new() { Type = "string", Description = "شناسه یکتای دسته‌بندی کالا (در صورت وجود)" }
                    }
                }
            },
            new McpToolDefinition
            {
                Name = "drill_down_sold_items",
                Description = "گزارش و ریز اقلام فروخته‌شده در فاکتورها به همراه نام کالا، بارکد، وزن واقعی (گرم)، نام خریدار، شماره فاکتور، تاریخ و لینک دانلود PDF فاکتور با امکان فیلتر بر اساس دسته‌بندی، نام کالا و بازه تاریخ (امکان پیگیری و Drill-down بعد از دیدن سرجمع فروش دسته‌ها)",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["categoryName"] = new() { Type = "string", Description = "نام دسته‌بندی کالا جهت فیلتر (مثلاً 'انگشتر' یا 'النگو')" },
                        ["categoryId"] = new() { Type = "string", Description = "شناسه یکتای دسته‌بندی" },
                        ["query"] = new() { Type = "string", Description = "متن جستجو در نام کالا، بارکد، شماره فاکتور یا نام مشتری (مثلاً 'کارتیه')" },
                        ["fromDate"] = new() { Type = "string", Description = "از تاریخ (شمسی مثلاً 1404/05/01 یا میلادی)" },
                        ["toDate"] = new() { Type = "string", Description = "تا تاریخ (شمسی مثلاً 1404/05/31 یا میلادی)" },
                        ["pageSize"] = new() { Type = "number", Description = "تعداد ردیف‌ها (پیش‌فرض 30، حداکثر 200)" },
                        ["page"] = new() { Type = "number", Description = "شماره صفحه (پیش‌فرض 1)" }
                    }
                }
            },
            new McpToolDefinition
            {
                Name = "compare_category_sales",
                Description = "مقایسه تحلیلی و آماری فروش دسته‌بندی‌های کالا بین دو بازه زمانی (مثلاً ماه جاری در مقایسه با ماه قبل، یا دو فصل) به تفکیک وزن واقعی طلا (گرم)، تعداد و مبلغ فروش با محاسبه دقیق درصد رشد یا افت (Δ%) هر دسته",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["period"] = new() { Type = "string", Description = "نوع دوره (MonthOverMonth: مقایسه ماه جاری با ماه قبل بر اساس تقویم شمسی، Custom: بازه دلخواه)", Enum = ["MonthOverMonth", "Custom"] },
                        ["fromDate"] = new() { Type = "string", Description = "تاریخ شروع دوره اصلی (شمسی مثلاً 1404/05/01)" },
                        ["toDate"] = new() { Type = "string", Description = "تاریخ پایان دوره اصلی (شمسی مثلاً 1404/05/31)" },
                        ["previousFromDate"] = new() { Type = "string", Description = "تاریخ شروع دوره مقایسه (شمسی مثلاً 1404/04/01)" },
                        ["previousToDate"] = new() { Type = "string", Description = "تاریخ پایان دوره مقایسه (شمسی مثلاً 1404/04/31)" },
                        ["categoryName"] = new() { Type = "string", Description = "فیلتر دسته‌بندی خاص در صورت تمایل (اختیاری)" }
                    }
                }
            },
            new McpToolDefinition
            {
                Name = "get_invoice_details",
                Description = "دریافت جزئیات کامل یک فاکتور شامل اقلام طلا، طلای کهنه، سکه، ارز، دریافتی‌ها/پرداختی‌ها، اطلاعات مشتری، مانده حساب و لینک دانلود PDF فاکتور",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["invoiceId"] = new() { Type = "string", Description = "شناسه یکتای فاکتور" },
                        ["invoiceNumber"] = new() { Type = "number", Description = "شماره فاکتور" },
                        ["invoiceType"] = new() { Type = "string", Description = "نوع فاکتور (Sell: فروش، Purchase: خرید، Return: مرجوعی)", Enum = ["Sell", "Purchase", "Return"] }
                    }
                }
            },
            new McpToolDefinition
            {
                Name = "create_invoice",
                Description = "ثبت کامل فاکتور فروش، خرید یا مرجوعی طلا و جواهر به همراه اقلام طلا، طلای کهنه دریافتی، سکه، ارز، پرداختی‌های نقدی و کارتخوان با تولید لینک دانلود PDF",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["invoiceType"] = new() { Type = "string", Description = "نوع فاکتور (Sell: فروش، Purchase: خرید، Return: مرجوعی - پیش‌فرض Sell)", Enum = ["Sell", "Purchase", "Return"] },
                        ["customerName"] = new() { Type = "string", Description = "نام مشتری (مثلاً مسعود خدادادی)" },
                        ["customerPhone"] = new() { Type = "string", Description = "شماره همراه مشتری (جهت شناسایی یا ثبت خودکار)" },
                        ["customerId"] = new() { Type = "string", Description = "شناسه یکتای مشتری در صورت وجود" },
                        ["items"] = new() { Type = "array", Description = "لیست اقلام طلا و جواهر نو (شامل نام کالا، وزن به گرم، نرخ گرم، اجرت درصدی یا ثابت، سود و مالیات)" },
                        ["usedGoldItems"] = new() { Type = "array", Description = "لیست اقلام طلای کهنه/متفرقه دریافتی از مشتری (شامل شرح، وزن به گرم، کسری عیار از 750 و نرخ گرم)" },
                        ["coinItems"] = new() { Type = "array", Description = "لیست اقلام سکه در فاکتور (شامل عنوان سکه، تعداد و قیمت واحد)" },
                        ["currencyItems"] = new() { Type = "array", Description = "لیست اقلام ارزی در فاکتور (شامل عنوان ارز، مقدار و نرخ واحد)" },
                        ["payments"] = new() { Type = "array", Description = "لیست مبالغ دریافتی یا پرداختی (شامل نوع پرداخت Cash/Pos/Check/BankTransfer، مبلغ، حساب مالی و شماره پیگیری)" },
                        ["discounts"] = new() { Type = "array", Description = "لیست تخفیف‌ها (شامل مبلغ و شرح)" },
                        ["extraCosts"] = new() { Type = "array", Description = "لیست هزینه‌های اضافی (شامل مبلغ و شرح)" },
                        ["note"] = new() { Type = "string", Description = "توضیحات کلی فاکتور" }
                    },
                    Required = ["customerName"]
                }
            },
            new McpToolDefinition
            {
                Name = "add_invoice_payment",
                Description = "ثبت دریافت یا پرداخت وجه (نقد، پوز/کارتخوان، حواله، چک) برای یک فاکتور موجود و به‌روزرسانی مانده حساب فاکتور",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["invoiceNumber"] = new() { Type = "number", Description = "شماره فاکتور مورد نظر (مثلاً 120)" },
                        ["invoiceId"] = new() { Type = "string", Description = "شناسه یکتای فاکتور در صورت وجود" },
                        ["invoiceType"] = new() { Type = "string", Description = "نوع فاکتور (پیش‌فرض Sell)", Enum = ["Sell", "Purchase", "Return"] },
                        ["paymentType"] = new() { Type = "string", Description = "نوع پرداخت (Cash: نقد، Pos: کارتخوان، BankTransfer: حواله، Check: چک)", Enum = ["Cash", "Pos", "BankTransfer", "Check"] },
                        ["amount"] = new() { Type = "number", Description = "مبلغ پرداختی" },
                        ["financialAccountTitle"] = new() { Type = "string", Description = "عنوان حساب مالی/بانک (مثلاً بانک ملت یا صندوق)" },
                        ["referenceNumber"] = new() { Type = "string", Description = "شماره پیگیری، ارجاع یا رسید کارتخوان" },
                        ["note"] = new() { Type = "string", Description = "توضیحات پرداخت" },
                        ["checkNumber"] = new() { Type = "string", Description = "شماره چک (در صورت پرداخت با چک)" },
                        ["checkSayadiCode"] = new() { Type = "string", Description = "شناسه ۱۶ رقمی صیادی چک" },
                        ["checkDueDate"] = new() { Type = "string", Description = "تاریخ سررسید چک" }
                    },
                    Required = ["amount"]
                }
            },
            new McpToolDefinition
            {
                Name = "get_trial_balance_report",
                Description = "گزارش تراز آزمایشی حساب‌های کل، معین و تفصیلی فروشگاه",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["fromDate"] = new() { Type = "string", Description = "از تاریخ (شمسی یا میلادی)" },
                        ["toDate"] = new() { Type = "string", Description = "تا تاریخ (شمسی یا میلادی)" }
                    }
                }
            },
            new McpToolDefinition
            {
                Name = "get_used_gold_hidden_profit",
                Description = "گزارش سود پنهان ناشی از آب کردن طلای کهنه و تفاوت عیار اسمی با عیار تعیین‌شده در آزمایشگاه ری‌گیری",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["fromDate"] = new() { Type = "string", Description = "از تاریخ (شمسی یا میلادی)" },
                        ["toDate"] = new() { Type = "string", Description = "تا تاریخ (شمسی یا میلادی)" }
                    }
                }
            }
        ];
    }

    public List<McpResourceDefinition> GetResources()
    {
        return
        [
            new McpResourceDefinition
            {
                Uri = "goldex://prices/live",
                Name = "نرخ‌های لحظه‌ای بازار طلا و ارز",
                Description = "اطلاعات زنده قیمت‌های طلا، سکه و ارزهای فعال در فروشگاه",
                MimeType = "application/json"
            },
            new McpResourceDefinition
            {
                Uri = "goldex://store/info",
                Name = "اطلاعات فروشگاه فعال",
                Description = "مشخصات فروشگاه یا شعبه جاری فعال در نشست MCP",
                MimeType = "application/json"
            }
        ];
    }

    public List<McpPromptDefinition> GetPrompts()
    {
        return
        [
            new McpPromptDefinition
            {
                Name = "category-sales-analysis",
                Description = "تحلیل و بررسی میزان فروش دسته‌بندی‌های مختلف طلا (انگشتر، النگو، دستبند و...) بر اساس وزن، تعداد و مبلغ",
                Arguments =
                [
                    new McpPromptArgument { Name = "period", Description = "دوره زمانی (مثلاً مرداد ماه)", Required = false },
                    new McpPromptArgument { Name = "categoryName", Description = "نام دسته‌بندی خاص در صورت تمایل", Required = false }
                ]
            },
            new McpPromptDefinition
            {
                Name = "sales-performance-analysis",
                Description = "تحلیل عملکرد و رشد فروش ماه جاری در مقایسه با ماه گذشته با شاخص‌های کلیدی (KPIs)",
                Arguments =
                [
                    new McpPromptArgument { Name = "period", Description = "دوره زمانی (مثلاً ماه جاری نسبت به ماه قبل)", Required = false }
                ]
            },
            new McpPromptDefinition
            {
                Name = "create-gold-sale-invoice",
                Description = "راهنمای ثبت هوشمند فاکتور فروش طلا به همراه محاسبات اجرت، سود و طلای کهنه تعویضی",
                Arguments =
                [
                    new McpPromptArgument { Name = "customerName", Description = "نام مشتری", Required = true },
                    new McpPromptArgument { Name = "productDetails", Description = "شرح طلا، وزن و اجرت", Required = true }
                ]
            },
            new McpPromptDefinition
            {
                Name = "audit-customer-ledger",
                Description = "تحلیل و بررسی جامع وضعیت حساب، بدهی‌ها، طلب‌ها و فاکتورهای باز یک مشتری",
                Arguments =
                [
                    new McpPromptArgument { Name = "customerName", Description = "نام مشتری", Required = true }
                ]
            },
            new McpPromptDefinition
            {
                Name = "calculate-jewelry-quote",
                Description = "محاسبه پیش‌فاکتور و مظنه قیمت طلا و جواهر بر اساس نرخ لحظه‌ای روز",
                Arguments =
                [
                    new McpPromptArgument { Name = "productDescription", Description = "شرح طلا یا جواهر (وزن، اجرت)", Required = true }
                ]
            }
        ];
    }

    public async Task<McpContentResult> CallToolAsync(string name, JsonElement arguments, CancellationToken cancellationToken = default)
    {
        try
        {
            return name switch
            {
                "get_live_gold_prices" => await ExecuteGetLiveGoldPricesAsync(arguments, cancellationToken),
                "calculate_gold_product_price" => ExecuteCalculateGoldProductPrice(arguments),
                "calculate_scrap_gold_valuation" => ExecuteCalculateScrapGoldValuation(arguments),
                "calculate_molten_gold" => ExecuteCalculateMoltenGold(arguments),
                "search_inventory_stock" => await ExecuteSearchInventoryStockAsync(arguments, cancellationToken),
                "get_customer_balance" => await ExecuteGetCustomerBalanceAsync(arguments, cancellationToken),
                "search_customers" => await ExecuteSearchCustomersAsync(arguments, cancellationToken),
                "create_customer" => await ExecuteCreateCustomerAsync(arguments, cancellationToken),
                "get_customer_statement" => await ExecuteGetCustomerStatementAsync(arguments, cancellationToken),
                "search_invoices" => await ExecuteSearchInvoicesAsync(arguments, cancellationToken),
                "get_sales_performance_report" => await ExecuteGetSalesPerformanceReportAsync(arguments, cancellationToken),
                "get_category_sales_summary" => await ExecuteGetCategorySalesSummaryAsync(arguments, cancellationToken),
                "drill_down_sold_items" => await ExecuteDrillDownSoldItemsAsync(arguments, cancellationToken),
                "compare_category_sales" => await ExecuteCompareCategorySalesAsync(arguments, cancellationToken),
                "get_invoice_details" => await ExecuteGetInvoiceDetailsAsync(arguments, cancellationToken),
                "create_invoice" => await ExecuteCreateInvoiceAsync(arguments, cancellationToken),
                "add_invoice_payment" => await ExecuteAddInvoicePaymentAsync(arguments, cancellationToken),
                "get_trial_balance_report" => await ExecuteGetTrialBalanceReportAsync(arguments, cancellationToken),
                "get_used_gold_hidden_profit" => await ExecuteGetUsedGoldHiddenProfitAsync(arguments, cancellationToken),
                _ => McpContentResult.Text($"ابزار ناشناخته: {name}", isError: true)
            };
        }
        catch (Exception ex)
        {
            return McpContentResult.Text($"خطا در اجرای ابزار {name}: {ex.Message}", isError: true);
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // Tool Executions
    // ----------------------------------------------------------------------------------------------------

    private async Task<McpContentResult> ExecuteGetLiveGoldPricesAsync(JsonElement args, CancellationToken ct)
    {
        bool? isPinned = args.TryGetProperty("isPinnedOnly", out var p) && p.ValueKind == JsonValueKind.True;
        var prices = await priceService.GetListAsync(isPinned, ct);

        if (prices.Count == 0)
            return McpContentResult.Text("هیچ قیمتی در سیستم ثبت نشده یا هنوز به‌روزرسانی نشده است.");

        var sb = new StringBuilder();
        sb.AppendLine("### 📈 آخرین نرخ‌های لحظه‌ای طلا، سکه و ارز:");
        sb.AppendLine("| عنوان نماد | نرخ | تغییر | تاریخ به‌روزرسانی |");
        sb.AppendLine("| :--- | :--- | :--- | :--- |");

        foreach (var price in prices)
        {
            var dateStr = price.LastUpdate.HasValue
                ? price.LastUpdate.Value.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture)
                : "-";

            var changeStr = !string.IsNullOrEmpty(price.Change) ? price.Change : "-";

            sb.AppendLine($"| {price.Title} | **{price.Value}** {price.Unit} | {changeStr} | {dateStr} |");
        }

        return McpContentResult.Text(sb.ToString());
    }

    private static McpContentResult ExecuteCalculateGoldProductPrice(JsonElement args)
    {
        if (!args.TryGetProperty("weight", out var wProp) ||
            !args.TryGetProperty("fineness", out var fProp) ||
            !args.TryGetProperty("gramPrice", out var gpProp))
        {
            return McpContentResult.Text("پارامترهای وزن (weight)، عیار (fineness) و نرخ گرم (gramPrice) الزامی هستند.", isError: true);
        }

        var weight = wProp.GetDecimal();
        var fineness = fProp.GetDecimal();
        var gramPrice = gpProp.GetDecimal();

        var wageType = args.TryGetProperty("wageType", out var wtProp) && wtProp.GetString()?.Equals("Fixed", StringComparison.OrdinalIgnoreCase) == true
            ? WageType.Fixed
            : WageType.Percent;

        var wageAmount = args.TryGetProperty("wageAmount", out var waProp) ? waProp.GetDecimal() : 0m;
        var profitPercent = args.TryGetProperty("profitPercent", out var ppProp) ? ppProp.GetDecimal() : 7m;
        var taxPercent = args.TryGetProperty("taxPercent", out var tpProp) ? tpProp.GetDecimal() : 9m;
        var stoneAmount = args.TryGetProperty("stoneAmount", out var saProp) ? saProp.GetDecimal() : 0m;

        // 1. Raw gold value
        var rawAmount = weight * (fineness / 750m) * gramPrice;

        // 2. Wage
        var calculatedWage = wageType == WageType.Percent
            ? rawAmount * (wageAmount / 100m)
            : wageAmount * weight;

        // 3. Profit (calculated on raw + wage + stone)
        var profitBase = rawAmount + calculatedWage + stoneAmount;
        var calculatedProfit = profitBase * (profitPercent / 100m);

        // 4. Tax (VAT on wage + profit per Iran gold regulations)
        var taxBase = calculatedWage + calculatedProfit;
        var calculatedTax = taxBase * (taxPercent / 100m);

        // 5. Total
        var finalTotal = rawAmount + calculatedWage + calculatedProfit + calculatedTax + stoneAmount;

        var sb = new StringBuilder();
        sb.AppendLine("### 🧮 محاسبه قیمت طلا و جواهر:");
        sb.AppendLine($"- **وزن:** {weight:N3} گرم (عیار {fineness})");
        sb.AppendLine($"- **نرخ مبنای هر گرم:** {gramPrice:N0}");
        sb.AppendLine($"- **قیمت طلای خام:** {rawAmount:N0}");
        sb.AppendLine($"- **اجرت ساخت ({wageAmount}{(wageType == WageType.Percent ? "%" : " مبلغ ثابت")}):** {calculatedWage:N0}");
        if (stoneAmount > 0) sb.AppendLine($"- **ارزش سنگ/نگین:** {stoneAmount:N0}");
        sb.AppendLine($"- **سود فروشنده ({profitPercent}%):** {calculatedProfit:N0}");
        sb.AppendLine($"- **مالیات بر ارزش افزوده ({taxPercent}% روی سود و اجرت):** {calculatedTax:N0}");
        sb.AppendLine($"---\n### 💎 **مبلغ نهایی قابل پرداخت:** {finalTotal:N0}");

        return McpContentResult.Text(sb.ToString());
    }

    private static McpContentResult ExecuteCalculateScrapGoldValuation(JsonElement args)
    {
        if (!args.TryGetProperty("weight", out var wProp) ||
            !args.TryGetProperty("gramPrice750", out var gpProp))
        {
            return McpContentResult.Text("پارامترهای وزن (weight) و نرخ روز گرم ۷۵۰ (gramPrice750) الزامی هستند.", isError: true);
        }

        var weight = wProp.GetDecimal();
        var fineness = args.TryGetProperty("fineness", out var fProp) ? fProp.GetDecimal() : 750m;
        var deduction = args.TryGetProperty("deductionFrom750", out var dProp) ? dProp.GetDecimal() : 0m;
        var gramPrice = gpProp.GetDecimal();

        var effectiveFineness = Math.Max(0, fineness - deduction);
        var equivalent18KWeight = weight * (effectiveFineness / 750m);
        var totalValue = equivalent18KWeight * gramPrice;

        var sb = new StringBuilder();
        sb.AppendLine("### ♻️ محاسبه ارزش خرید طلای کهنه و متفرقه:");
        sb.AppendLine($"- **وزن طلای کهنه:** {weight:N3} گرم");
        sb.AppendLine($"- **عیار اسمی:** {fineness}");
        sb.AppendLine($"- **کسری عیار از ۷۵۰:** {deduction} (عیار موثر: {effectiveFineness})");
        sb.AppendLine($"- **معادل وزنی طلای ۱۸ عیار (۷۵۰):** {equivalent18KWeight:N3} گرم");
        sb.AppendLine($"- **نرخ هر گرم ۱۸ عیار:** {gramPrice:N0}");
        sb.AppendLine($"---\n### 💰 **ارزش کل طلای کهنه جهت پرداخت یا تهاتر:** {totalValue:N0}");

        return McpContentResult.Text(sb.ToString());
    }

    private static McpContentResult ExecuteCalculateMoltenGold(JsonElement args)
    {
        if (!args.TryGetProperty("fineness", out var fProp) ||
            !args.TryGetProperty("gramPrice", out var gpProp))
        {
            return McpContentResult.Text("پارامترهای عیار آزمایشگاه (fineness) و نرخ هر گرم طلای ۷۵۰ (gramPrice) الزامی هستند.", isError: true);
        }

        var fineness = fProp.GetDecimal();
        var gramPrice = gpProp.GetDecimal();

        if (args.TryGetProperty("targetBudget", out var tbProp) && tbProp.GetDecimal() > 0)
        {
            var budget = tbProp.GetDecimal();
            var pricePerGramOfThisFineness = gramPrice * (fineness / 750m);
            var purchasableWeight = budget / pricePerGramOfThisFineness;

            return McpContentResult.Text($"### 🪙 محاسبه خرید طلای آب‌شده با بودجه معین:\n" +
                                         $"- **بودجه کل:** {budget:N0}\n" +
                                         $"- **عیار ری‌گیری:** {fineness}\n" +
                                         $"- **نرخ هر گرم این عیار:** {pricePerGramOfThisFineness:N0}\n" +
                                         $"---\n" +
                                         $"### ✨ **وزن قابل خرید:** **{purchasableWeight:N3} گرم**");
        }

        if (args.TryGetProperty("weight", out var wProp))
        {
            var weight = wProp.GetDecimal();
            var equivalentWeight750 = weight * (fineness / 750m);
            var totalValue = equivalentWeight750 * gramPrice;

            return McpContentResult.Text($"### 🪙 محاسبه ارزش قطعه طلای آب‌شده:\n" +
                                         $"- **وزن قطعه:** {weight:N3} گرم\n" +
                                         $"- **عیار ری‌گیری:** {fineness}\n" +
                                         $"- **معادل وزنی طلای ۷۵۰ (۱۸ عیار):** {equivalentWeight750:N3} گرم\n" +
                                         $"---\n" +
                                         $"### 💰 **ارزش کل:** **{totalValue:N0}**");
        }

        return McpContentResult.Text("لطفاً یا وزن قطعه (weight) یا بودجه خرید (targetBudget) را ارسال فرمایید.", isError: true);
    }

    private async Task<McpContentResult> ExecuteSearchInventoryStockAsync(JsonElement args, CancellationToken ct)
    {
        var query = args.TryGetProperty("query", out var qProp) ? qProp.GetString() : null;
        var pageSize = args.TryGetProperty("pageSize", out var psProp) ? psProp.GetInt32() : 15;

        var filter = new RequestFilter(Skip: 0, Take: pageSize, Search: query);
        var inventoryFilter = new InventoryFilter(null, null, null, null, null);
        var stocks = await inventoryStockService.GetListAsync(filter, inventoryFilter, ct);

        if (stocks.Data.Count == 0)
            return McpContentResult.Text($"هیچ کالایی با عبارت جستجوی «{query}» در انبار یافت نشد.");

        var sb = new StringBuilder();
        sb.AppendLine($"### 📦 موجودی انبار ({stocks.Total} مورد یافت شد):");
        sb.AppendLine("| عنوان کالا | بارکد | موجودی / وزن | واحد |");
        sb.AppendLine("| :--- | :--- | :--- | :--- |");

        foreach (var s in stocks.Data)
        {
            var title = s.Product?.Name ?? s.Coin?.Coin?.Title ?? s.Currency?.Title ?? "کالای انبار";
            var barcode = s.Product?.Barcode ?? s.Coin?.Barcode ?? "-";
            var unit = s.Product != null ? "گرم" : (s.Coin != null ? "عدد" : (s.Currency?.Title ?? "واحد"));
            sb.AppendLine($"| {title} | `{barcode}` | **{s.CurrentAmount:N3}** | {unit} |");
        }

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteGetCustomerBalanceAsync(JsonElement args, CancellationToken ct)
    {
        Guid? customerId = null;
        string? customerName = null;

        if (args.TryGetProperty("customerId", out var idProp) && Guid.TryParse(idProp.GetString(), out var id))
        {
            customerId = id;
        }
        else if (args.TryGetProperty("customerName", out var nameProp))
        {
            customerName = nameProp.GetString();
            var matches = await customerService.GetByNameAsync(customerName, null, ct);
            var first = matches.FirstOrDefault();
            if (first != null)
            {
                customerId = first.Id;
                customerName = first.FullName;
            }
        }

        if (!customerId.HasValue)
            return McpContentResult.Text("مشتری مورد نظر با نام یا شناسه ارسالی یافت نشد.", isError: true);

        var balances = await transactionService.GetCustomerRemainingListAsync(customerId.Value, null, null, null, ct);

        if (balances.Count == 0)
            return McpContentResult.Text($"مشتری «{customerName}» در حال حاضر هیچ مانده حساب یا تراکنشی در سیستم ندارد (تراز صفر).");

        var sb = new StringBuilder();
        sb.AppendLine($"### 👤 وضعیت حساب مشتری: **{customerName}**");
        sb.AppendLine("| واحد پولی / وزنی | مانده حساب | وضعیت |");
        sb.AppendLine("| :--- | :--- | :--- |");

        foreach (var b in balances)
        {
            var status = b.Amount switch
            {
                > 0 => "🔴 بدهکار به فروشگاه",
                < 0 => "🟢 بستانکار از فروشگاه",
                _ => "⚪ تسویه"
            };

            var formattedAmount = Math.Abs(b.Amount).ToString("N2");
            sb.AppendLine($"| {b.PriceUnit.Title} | **{formattedAmount}** | {status} |");
        }

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteSearchCustomersAsync(JsonElement args, CancellationToken ct)
    {
        var query = args.TryGetProperty("query", out var qProp) ? qProp.GetString() : "";
        if (string.IsNullOrWhiteSpace(query))
            return McpContentResult.Text("عبارت جستجو (query) الزامی است.", isError: true);

        var matches = await customerService.GetByNameAsync(query, null, ct);

        if (matches.Count == 0)
            return McpContentResult.Text($"هیچ مشتری با عبارت «{query}» پیدا نشد.");

        var sb = new StringBuilder();
        sb.AppendLine($"### 👥 نتایج جستجوی مشتریان ({matches.Count} مورد):");
        sb.AppendLine("| نام و نام خانوادگی | شماره همراه | کد ملی | شناسه یکتا |");
        sb.AppendLine("| :--- | :--- | :--- | :--- |");

        foreach (var c in matches.Take(15))
        {
            sb.AppendLine($"| **{c.FullName}** | {c.PhoneNumber ?? "-"} | {c.NationalId ?? "-"} | `{c.Id}` |");
        }

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteCreateCustomerAsync(JsonElement args, CancellationToken ct)
    {
        if (!args.TryGetProperty("fullName", out var fnProp) || string.IsNullOrWhiteSpace(fnProp.GetString()))
        {
            return McpContentResult.Text("نام و نام خانوادگی مشتری (fullName) الزامی است.", isError: true);
        }

        var fullName = fnProp.GetString()!.Trim();
        var phone = args.TryGetProperty("phoneNumber", out var pProp) ? pProp.GetString() : "";
        var address = args.TryGetProperty("address", out var aProp) ? aProp.GetString() : null;
        var nationalId = args.TryGetProperty("nationalId", out var nidProp) ? nidProp.GetString() : null;

        var customerType = CustomerType.RetailCustomer;
        if (args.TryGetProperty("customerType", out var ctProp) && Enum.TryParse<CustomerType>(ctProp.GetString(), true, out var parsedCt))
        {
            customerType = parsedCt;
        }

        if (string.IsNullOrWhiteSpace(nationalId))
        {
            var genNat = await customerService.GenerateNationalIdAsync(ct);
            nationalId = genNat.NationalId;
        }

        var defaultPriceUnit = await priceUnitService.GetDefaultAsync(ct);

        var requestDto = new CustomerRequestDto(
            Id: null,
            FullName: fullName,
            NationalId: nationalId,
            PhoneNumber: phone ?? "",
            Address: address,
            CreditLimit: null,
            CreditLimitPriceUnitId: defaultPriceUnit?.Id,
            CustomerType: customerType);

        var newId = await customerService.CreateAsync(requestDto, ct);

        return McpContentResult.Text($"✅ **مشتری جدید با موفقیت ثبت شد:**\n" +
                                     $"- **نام:** {fullName}\n" +
                                     $"- **شماره همراه:** {phone ?? "-"}\n" +
                                     $"- **کد ملی / شناسه:** {nationalId}\n" +
                                     $"- **شناسه سیستمی:** `{newId}`");
    }

    private async Task<McpContentResult> ExecuteGetCustomerStatementAsync(JsonElement args, CancellationToken ct)
    {
        Guid? customerId = null;

        if (args.TryGetProperty("customerId", out var cidProp) && Guid.TryParse(cidProp.GetString(), out var parsedId))
        {
            customerId = parsedId;
        }
        else if (args.TryGetProperty("customerName", out var cnameProp))
        {
            var name = cnameProp.GetString();
            var matches = await customerService.GetByNameAsync(name, null, ct);
            var first = matches.FirstOrDefault();
            if (first != null)
                customerId = first.Id;
        }

        if (!customerId.HasValue)
            return McpContentResult.Text("مشتری مورد نظر یافت نشد.", isError: true);

        var fromDateStr = args.TryGetProperty("fromDate", out var fdP) ? fdP.GetString() : null;
        var toDateStr = args.TryGetProperty("toDate", out var tdP) ? tdP.GetString() : null;

        var start = ParseDate(fromDateStr);
        var end = ParseDate(toDateStr);

        var request = new CustomerTransactionRpRequest(customerId.Value, null, null, start, end);
        var transactions = await reportingService.GetCustomerTransactionsAsync(request, ct);

        if (transactions.Count == 0)
            return McpContentResult.Text("هیچ تراکنشی برای این مشتری در بازه زمانی مورد نظر ثبت نشده است.");

        var sb = new StringBuilder();
        sb.AppendLine($"### 📜 ریز تراکنش‌های دفتر کل مشتری ({transactions.Count} تراکنش):");
        sb.AppendLine("| تاریخ | شرح تراکنش | بدهکار | بستانکار | مانده | واحد |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- |");

        foreach (var t in transactions.Take(50))
        {
            var dateStr = t.PostingDate.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            var debitStr = t.TransactionType == TransactionType.Debit ? t.Amount.ToString("N2") : "-";
            var creditStr = t.TransactionType == TransactionType.Credit ? t.Amount.ToString("N2") : "-";
            sb.AppendLine($"| {dateStr} | {t.Description} | {debitStr} | {creditStr} | {t.RunningBalance:N2} | {t.PriceUnitTitle} |");
        }

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteSearchInvoicesAsync(JsonElement args, CancellationToken ct)
    {
        var fromDateStr = args.TryGetProperty("fromDate", out var fdP) ? fdP.GetString() : null;
        var toDateStr = args.TryGetProperty("toDate", out var tdP) ? tdP.GetString() : null;
        var query = args.TryGetProperty("query", out var qProp) ? qProp.GetString() : null;
        var pageSize = args.TryGetProperty("pageSize", out var psProp) ? Math.Clamp(psProp.GetInt32(), 1, 500) : 50;
        var page = args.TryGetProperty("page", out var pgProp) ? Math.Max(1, pgProp.GetInt32()) : 1;

        var startDate = ParseDate(fromDateStr);
        var endDate = ParseDate(toDateStr);

        InvoiceType? targetInvoiceType = null;
        if (args.TryGetProperty("invoiceType", out var itProp) && Enum.TryParse<InvoiceType>(itProp.GetString(), true, out var it))
            targetInvoiceType = it;

        var skip = (page - 1) * pageSize;
        var filter = new RequestFilter(Skip: skip, Take: pageSize, Search: query);
        var invoiceFilter = new InvoiceFilter(null, targetInvoiceType, null, startDate, endDate);
        var list = await invoiceService.GetListAsync(filter, invoiceFilter, null, ct);

        if (list.Data.Count == 0)
            return McpContentResult.Text("هیچ فاکتوری با فیلترهای مشخص‌شده در این بازه یافت نشد.");

        var totalSum = list.Data.Sum(x => x.TotalAmount);
        var totalUnpaid = list.Data.Sum(x => x.TotalUnpaidAmount);
        var totalPaid = totalSum - totalUnpaid;
        var avgAmount = list.Data.Count > 0 ? totalSum / list.Data.Count : 0;
        var unitTitle = list.Data.FirstOrDefault()?.PriceUnit ?? "تومان";

        var sb = new StringBuilder();
        sb.AppendLine($"# 🧾 نتایج جستجوی فاکتورها (کل: {list.Total} مورد - نمایش {list.Data.Count} ردیف در صفحه {page})");
        
        // Summary Card
        sb.AppendLine("### 📊 خلاصه آماری فاکتورهای این بازه:");
        sb.AppendLine($"- **مجموع کل مبالغ:** **{totalSum:N0} {unitTitle}**");
        sb.AppendLine($"- **مجموع مبالغ تسویه‌شده:** **{totalPaid:N0} {unitTitle}**");
        sb.AppendLine($"- **مجموع مانده تسویه‌نشده:** **{totalUnpaid:N0} {unitTitle}**");
        sb.AppendLine($"- **میانگین مبلغ هر فاکتور:** **{avgAmount:N0} {unitTitle}**\n");

        sb.AppendLine("| شماره فاکتور | نوع | مشتری | تاریخ | مبلغ کل | واحد | وضعیت پرداخت | دانلود |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |");

        var baseUrl = GetBaseUrl();
        foreach (var inv in list.Data)
        {
            var dateStr = inv.InvoiceDate.ToString("yyyy/MM/dd");
            var pdfLink = $"[📥 PDF]({baseUrl}/api/invoices/{inv.Id}/download-pdf)";
            sb.AppendLine($"| {inv.InvoiceNumber} | {inv.InvoiceType} | {inv.CustomerFullName} | {dateStr} | {inv.TotalAmount:N0} | {inv.PriceUnit} | {inv.PaymentStatus} | {pdfLink} |");
        }

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteGetSalesPerformanceReportAsync(JsonElement args, CancellationToken ct)
    {
        var period = args.TryGetProperty("period", out var pProp) ? pProp.GetString() : "MonthOverMonth";
        var fromDateStr = args.TryGetProperty("fromDate", out var fdP) ? fdP.GetString() : null;
        var toDateStr = args.TryGetProperty("toDate", out var tdP) ? tdP.GetString() : null;
        var prevFromDateStr = args.TryGetProperty("previousFromDate", out var pfdP) ? pfdP.GetString() : null;
        var prevToDateStr = args.TryGetProperty("previousToDate", out var ptdP) ? ptdP.GetString() : null;

        DateTime start1, end1;
        DateTime? start2 = null, end2 = null;
        string title1, title2 = "";

        if (period?.Equals("CurrentMonth", StringComparison.OrdinalIgnoreCase) == true)
        {
            (start1, end1, title1) = GetShamsiMonthRange(0);
        }
        else if (period?.Equals("PreviousMonth", StringComparison.OrdinalIgnoreCase) == true)
        {
            (start1, end1, title1) = GetShamsiMonthRange(-1);
        }
        else if (period?.Equals("Custom", StringComparison.OrdinalIgnoreCase) == true && !string.IsNullOrWhiteSpace(fromDateStr) && !string.IsNullOrWhiteSpace(toDateStr))
        {
            start1 = ParseDate(fromDateStr) ?? DateTime.UtcNow.AddMonths(-1);
            end1 = ParseDate(toDateStr) ?? DateTime.UtcNow;
            title1 = $"{fromDateStr} تا {toDateStr}";

            if (!string.IsNullOrWhiteSpace(prevFromDateStr) && !string.IsNullOrWhiteSpace(prevToDateStr))
            {
                start2 = ParseDate(prevFromDateStr);
                end2 = ParseDate(prevToDateStr);
                title2 = $"{prevFromDateStr} تا {prevToDateStr}";
            }
        }
        else
        {
            // Default: Month Over Month comparison (Current Month vs Previous Month)
            (start1, end1, title1) = GetShamsiMonthRange(0);
            var (s2, e2, t2) = GetShamsiMonthRange(-1);
            start2 = s2;
            end2 = e2;
            title2 = t2;
        }

        // Fetch Invoices for Period 1
        var invoices1 = await reportingService.GetSellInvoicesAsync(new SellInvoiceRpRequest(null, null, null, start1, end1), ct);

        // Fetch Invoices for Period 2 (if comparison)
        var invoices2 = (start2.HasValue && end2.HasValue)
            ? await reportingService.GetSellInvoicesAsync(new SellInvoiceRpRequest(null, null, null, start2.Value, end2.Value), ct)
            : [];

        // Compute Period 1 KPIs
        var count1 = invoices1.Count;
        var sales1 = invoices1.Sum(x => x.TotalPrice);
        var avgTicket1 = count1 > 0 ? sales1 / count1 : 0m;
        var profit1 = invoices1.Sum(x => x.TotalProfit);
        var wage1 = invoices1.Sum(x => x.TotalWage);
        var tax1 = invoices1.Sum(x => x.TotalTax);
        var weight1 = invoices1.Sum(x => x.TotalWeightEquivalent);
        var remaining1 = invoices1.Sum(x => x.RemainingPrice);
        var settledCount1 = invoices1.Count(x => x.RemainingPrice <= 0);
        var unitTitle = invoices1.FirstOrDefault()?.PriceUnit ?? "تومان";

        var sb = new StringBuilder();
        sb.AppendLine($"# 📊 گزارش تحلیل عملکرد فروش: **{title1}**" + (!string.IsNullOrEmpty(title2) ? $" در مقایسه با **{title2}**" : ""));

        if (start2.HasValue && end2.HasValue)
        {
            // Compute Period 2 KPIs
            var count2 = invoices2.Count;
            var sales2 = invoices2.Sum(x => x.TotalPrice);
            var avgTicket2 = count2 > 0 ? sales2 / count2 : 0m;
            var profit2 = invoices2.Sum(x => x.TotalProfit);
            var wage2 = invoices2.Sum(x => x.TotalWage);
            var weight2 = invoices2.Sum(x => x.TotalWeightEquivalent);

            // Compute Deltas
            var deltaSales = sales2 > 0 ? ((sales1 - sales2) / sales2) * 100m : 0m;
            var deltaCount = count2 > 0 ? (((decimal)count1 - count2) / count2) * 100m : 0m;
            var deltaAvg = avgTicket2 > 0 ? ((avgTicket1 - avgTicket2) / avgTicket2) * 100m : 0m;
            var deltaWeight = weight2 > 0 ? ((weight1 - weight2) / weight2) * 100m : 0m;
            var deltaProfit = profit2 > 0 ? ((profit1 - profit2) / profit2) * 100m : 0m;

            string FormatDelta(decimal delta) => delta switch
            {
                > 0 => $"🟢 **+{delta:F1}% رشد**",
                < 0 => $"🔴 **{delta:F1}% افت**",
                _ => "⚪ **بدون تغییر**"
            };

            sb.AppendLine("\n### 📈 جدول مقایسه شاخص‌های کلیدی عملکرد (KPIs):");
            sb.AppendLine($"| شاخص عملکرد | {title1} (دوره جاری) | {title2} (دوره قبل) | درصد تغییر ($\\\\Delta$) | وضعیت |");
            sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");
            sb.AppendLine($"| **مجموع فروش ناخالص** | **{sales1:N0}** {unitTitle} | {sales2:N0} {unitTitle} | {FormatDelta(deltaSales)} | {(deltaSales >= 0 ? "بهبود فروش" : "کاهش فروش")} |");
            sb.AppendLine($"| **تعداد فاکتورهای فروش** | **{count1}** عدد | {count2} عدد | {FormatDelta(deltaCount)} | {(deltaCount >= 0 ? "افزایش مشتریان" : "کاهش تعداد")} |");
            sb.AppendLine($"| **میانگین مبلغ هر فاکتور** | **{avgTicket1:N0}** {unitTitle} | {avgTicket2:N0} {unitTitle} | {FormatDelta(deltaAvg)} | {(deltaAvg >= 0 ? "سبد خرید بزرگتر" : "سبد خرید کوچکتر")} |");
            sb.AppendLine($"| **وزن طلای فروخته‌شده (معادل ۱۸)** | **{weight1:N3}** گرم | {weight2:N3} گرم | {FormatDelta(deltaWeight)} | {(deltaWeight >= 0 ? "افزایش حجم وزنی" : "کاهش حجم وزنی")} |");
            sb.AppendLine($"| **مجموع سود فروشنده** | **{profit1:N0}** {unitTitle} | {profit2:N0} {unitTitle} | {FormatDelta(deltaProfit)} | {(deltaProfit >= 0 ? "افزایش سودآوری" : "کاهش سودآوری")} |");
            sb.AppendLine($"| **مجموع اجرت ساخت** | **{wage1:N0}** {unitTitle} | {wage2:N0} {unitTitle} | - | - |");
        }
        else
        {
            sb.AppendLine("\n### 📋 خلاصه شاخص‌های عملکرد دوره:");
            sb.AppendLine($"- **مجموع کل فروش:** **{sales1:N0} {unitTitle}**");
            sb.AppendLine($"- **تعداد کل فاکتورها:** **{count1}** عدد");
            sb.AppendLine($"- **میانگین هر فاکتور:** **{avgTicket1:N0} {unitTitle}**");
            sb.AppendLine($"- **مجموع وزن طلای فروخته‌شده (معادل ۱۸ عیار):** **{weight1:N3}** گرم");
            sb.AppendLine($"- **مجموع سود حاصله:** **{profit1:N0} {unitTitle}**");
            sb.AppendLine($"- **مجموع اجرت ساخت:** **{wage1:N0} {unitTitle}**");
            sb.AppendLine($"- **مجموع مالیات ارزش افزوده:** **{tax1:N0} {unitTitle}**");
        }

        // Add Category Summary breakdown directly in the sales performance report
        var categorySummary = await reportingService.GetCategorySalesSummaryAsync(new CategorySalesRpRequest(start1, end1, null, null, null), ct);
        if (categorySummary.Count > 0)
        {
            sb.AppendLine("\n### 🏷️ تفکیک فروش برترین دسته‌بندی‌های کالا در دوره جاری:");
            sb.AppendLine("| ردیف | دسته‌بندی کالا | وزن (گرم) | سهم وزنی | تعداد | مبلغ فروش (تومان) |");
            sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- |");
            int catIdx = 1;
            foreach (var c in categorySummary.Take(6))
            {
                sb.AppendLine($"| {catIdx++} | **{c.CategoryTitle}** | **{c.TotalWeight:N3}** | {c.WeightPercentage:F1}% | {c.TotalQuantity} عدد | {c.TotalAmount:N0} |");
            }
        }

        // Settlement breakdown
        sb.AppendLine("\n### 💳 وضعیت تسویه و مطالبات فاکتورهای دوره جاری:");
        sb.AppendLine($"- **تعداد فاکتورهای تسویه‌شده کامل:** **{settledCount1}** از {count1} فاکتور ({(count1 > 0 ? (settledCount1 * 100.0 / count1) : 0):F1}%)");
        sb.AppendLine($"- **مانده مطالبات تسویه‌نشده:** **{remaining1:N0} {unitTitle}**");

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteGetCategorySalesSummaryAsync(JsonElement args, CancellationToken ct)
    {
        var period = args.TryGetProperty("period", out var pProp) ? pProp.GetString() : null;
        var fromDateStr = args.TryGetProperty("fromDate", out var fdP) ? fdP.GetString() : null;
        var toDateStr = args.TryGetProperty("toDate", out var tdP) ? tdP.GetString() : null;
        var categoryName = args.TryGetProperty("categoryName", out var cnP) ? cnP.GetString()?.Trim() : null;
        Guid? categoryId = null;
        if (args.TryGetProperty("categoryId", out var cidP) && Guid.TryParse(cidP.GetString(), out var parsedCid))
        {
            categoryId = parsedCid;
        }

        DateTime? start = null;
        DateTime? end = null;
        string periodTitle = "کل بازه زمانی";

        if (period?.Equals("CurrentMonth", StringComparison.OrdinalIgnoreCase) == true)
        {
            var (s, e, t) = GetShamsiMonthRange(0);
            start = s;
            end = e;
            periodTitle = t;
        }
        else if (period?.Equals("PreviousMonth", StringComparison.OrdinalIgnoreCase) == true)
        {
            var (s, e, t) = GetShamsiMonthRange(-1);
            start = s;
            end = e;
            periodTitle = t;
        }
        else if (!string.IsNullOrWhiteSpace(fromDateStr) || !string.IsNullOrWhiteSpace(toDateStr))
        {
            start = ParseDate(fromDateStr);
            end = ParseDate(toDateStr);
            periodTitle = $"{fromDateStr ?? "ابتدا"} تا {toDateStr ?? "اکنون"}";
        }
        else if (string.IsNullOrWhiteSpace(period) || period?.Equals("AllTime", StringComparison.OrdinalIgnoreCase) == false)
        {
            // If neither dates nor period specified, default to Current Month for rapid insights
            var (s, e, t) = GetShamsiMonthRange(0);
            start = s;
            end = e;
            periodTitle = t;
        }

        var request = new CategorySalesRpRequest(start, end, categoryId, categoryName, null);
        var categories = await reportingService.GetCategorySalesSummaryAsync(request, ct);

        if (categories.Count == 0)
        {
            var filterDesc = !string.IsNullOrWhiteSpace(categoryName) ? $" برای دسته‌بندی «{categoryName}»" : "";
            return McpContentResult.Text($"هیچ رکورد فروشی در دوره {periodTitle}{filterDesc} ثبت نشده است.");
        }

        var totalSumWeight = categories.Sum(x => x.TotalWeight);
        var totalSumQty = categories.Sum(x => x.TotalQuantity);
        var totalSumAmount = categories.Sum(x => x.TotalAmount);
        var totalSumProfit = categories.Sum(x => x.TotalProfit);
        var totalSumWage = categories.Sum(x => x.TotalWage);

        var sb = new StringBuilder();

        // If a specific category was searched and found
        if (!string.IsNullOrWhiteSpace(categoryName) && categories.Count == 1)
        {
            var item = categories[0];
            sb.AppendLine($"# 🏷️ گزارش فروش دسته‌بندی «{item.CategoryTitle}» ({periodTitle})");
            sb.AppendLine($"📊 **خلاصه شاخص‌های فروش این دسته:**");
            sb.AppendLine($"- **وزن کل فروخته‌شده (واقعی):** **{item.TotalWeight:N3} گرم** (وزن واقعی طلاهای فروخته‌شده)");
            sb.AppendLine($"- **تعداد کل فروخته‌شده:** **{item.TotalQuantity} عدد** (در قالب {item.ItemCount} ردیف فاکتور)");
            sb.AppendLine($"- **مجموع مبلغ فروش:** **{item.TotalAmount:N0} تومان**");
            sb.AppendLine($"- **مجموع سود فروشنده:** **{item.TotalProfit:N0} تومان**");
            sb.AppendLine($"- **مجموع اجرت ساخت:** **{item.TotalWage:N0} تومان**");
            sb.AppendLine($"- **مجموع مالیات:** **{item.TotalTax:N0} تومان**");
            if (item.WeightPercentage > 0)
            {
                sb.AppendLine($"- **سهم از کل فروش فروشگاه:** **{item.WeightPercentage:F1}% وزنی** | **{item.AmountPercentage:F1}% مبلغی**");
            }
            sb.AppendLine($"\n💡 *جهت مشاهده ریز فاکتورها و خریداران این دسته، می‌توانید از ابزار `drill_down_sold_items` با دسته‌بندی «{item.CategoryTitle}» استفاده فرمایید.*");
            return McpContentResult.Text(sb.ToString());
        }

        // Full multi-category breakdown
        sb.AppendLine($"# 🏷️ گزارش تجمیعی فروش به تفکیک دسته‌بندی کالا ({periodTitle})");
        sb.AppendLine($"📊 **مجموع کل فروش تمام دسته‌ها:** **{totalSumWeight:N3} گرم** | **{totalSumQty} عدد** | **{totalSumAmount:N0} تومان**\n");

        sb.AppendLine("| ردیف | دسته‌بندی کالا | وزن فروخته‌شده (گرم) | سهم وزنی | تعداد | مبلغ کل فروش (تومان) | سهم مبلغی | سود فروش | اجرت ساخت |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |");

        int index = 1;
        foreach (var c in categories)
        {
            sb.AppendLine($"| {index++} | **{c.CategoryTitle}** | **{c.TotalWeight:N3}** | {c.WeightPercentage:F1}% | {c.TotalQuantity} | **{c.TotalAmount:N0}** | {c.AmountPercentage:F1}% | {c.TotalProfit:N0} | {c.TotalWage:N0} |");
        }

        sb.AppendLine($"| **-** | **مجموع کل** | **{totalSumWeight:N3} گرم** | **100%** | **{totalSumQty} عدد** | **{totalSumAmount:N0} تومان** | **100%** | **{totalSumProfit:N0}** | **{totalSumWage:N0}** |");

        var topByWeight = categories.OrderByDescending(x => x.TotalWeight).FirstOrDefault();
        var topByAmount = categories.OrderByDescending(x => x.TotalAmount).FirstOrDefault();

        sb.AppendLine("\n### 💡 نکات کلیدی تحلیل فروش:");
        if (topByWeight != null)
            sb.AppendLine($"- 🥇 **بیشترین حجم وزنی فروش:** دسته‌بندی **«{topByWeight.CategoryTitle}»** با وزن **{topByWeight.TotalWeight:N3} گرم** ({topByWeight.WeightPercentage:F1}% کل فروش)");
        if (topByAmount != null)
            sb.AppendLine($"- 💰 **بیشترین ارزش ریالی فروش:** دسته‌بندی **«{topByAmount.CategoryTitle}»** با مبلغ **{topByAmount.TotalAmount:N0} تومان** ({topByAmount.AmountPercentage:F1}% کل فروش)");

        sb.AppendLine($"\n💡 *برای مشاهده ریز و مشخصات تک‌تک اقلام فروخته‌شده در هر دسته، می‌توانید از ابزار `drill_down_sold_items` استفاده فرمایید.*");

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteDrillDownSoldItemsAsync(JsonElement args, CancellationToken ct)
    {
        var fromDateStr = args.TryGetProperty("fromDate", out var fdP) ? fdP.GetString() : null;
        var toDateStr = args.TryGetProperty("toDate", out var tdP) ? tdP.GetString() : null;
        var categoryName = args.TryGetProperty("categoryName", out var cnP) ? cnP.GetString()?.Trim() : null;
        var query = args.TryGetProperty("query", out var qP) ? qP.GetString()?.Trim() : null;
        var pageSize = args.TryGetProperty("pageSize", out var psProp) ? Math.Clamp(psProp.GetInt32(), 1, 200) : 30;
        var page = args.TryGetProperty("page", out var pgProp) ? Math.Max(1, pgProp.GetInt32()) : 1;

        Guid? categoryId = null;
        if (args.TryGetProperty("categoryId", out var cidP) && Guid.TryParse(cidP.GetString(), out var parsedCid))
        {
            categoryId = parsedCid;
        }

        var start = ParseDate(fromDateStr);
        var end = ParseDate(toDateStr);
        var skip = (page - 1) * pageSize;

        var request = new SoldProductItemRpRequest(start, end, categoryId, categoryName, query, skip, pageSize);
        var items = await reportingService.GetSoldProductItemsAsync(request, ct);

        if (items.Count == 0)
        {
            var filterDesc = !string.IsNullOrWhiteSpace(categoryName) ? $" در دسته‌بندی «{categoryName}»" : "";
            return McpContentResult.Text($"هیچ قلم کالایی با فیلترهای مشخص‌شده{filterDesc} یافت نشد.");
        }

        var pageWeight = items.Sum(x => x.TotalWeight);
        var pageAmount = items.Sum(x => x.FinalAmount);
        var unitTitle = items.FirstOrDefault()?.PriceUnit ?? "تومان";

        var sb = new StringBuilder();
        var catHeader = !string.IsNullOrWhiteSpace(categoryName) ? $" در دسته‌بندی «{categoryName}»" : "";
        sb.AppendLine($"# 🔍 ریز اقلام فروخته‌شده{catHeader} (نمایش {items.Count} ردیف در صفحه {page})");
        sb.AppendLine($"📊 **مجموع این صفحه:** **{pageWeight:N3} گرم** | **{pageAmount:N0} {unitTitle}**\n");

        sb.AppendLine("| ردیف | شماره فاکتور | تاریخ | خریدار | عنوان کالا | بارکد | دسته‌بندی | وزن (گرم) | تعداد | نرخ گرم | سود و اجرت | مبلغ نهایی | دانلود PDF |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |");

        var baseUrl = GetBaseUrl();
        int index = skip + 1;

        foreach (var item in items)
        {
            var dateStr = item.InvoiceDate.ToString("yyyy/MM/dd");
            var barcodeStr = !string.IsNullOrEmpty(item.Barcode) ? $"`{item.Barcode}`" : "-";
            var customerStr = !string.IsNullOrEmpty(item.CustomerName) ? item.CustomerName : "عمومی";
            var profitWageStr = (item.WageAmount + item.ProfitAmount).ToString("N0");
            var pdfLink = $"[📥 PDF]({baseUrl}/api/invoices/{item.InvoiceId}/download-pdf)";

            sb.AppendLine($"| {index++} | {item.InvoiceNumber} | {dateStr} | {customerStr} | **{item.ProductName}** | {barcodeStr} | {item.CategoryTitle} | **{item.TotalWeight:N3}** | {item.Quantity} | {item.GramPrice:N0} | {profitWageStr} | **{item.FinalAmount:N0}** | {pdfLink} |");
        }

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteCompareCategorySalesAsync(JsonElement args, CancellationToken ct)
    {
        var period = args.TryGetProperty("period", out var pProp) ? pProp.GetString() : "MonthOverMonth";
        var fromDateStr = args.TryGetProperty("fromDate", out var fdP) ? fdP.GetString() : null;
        var toDateStr = args.TryGetProperty("toDate", out var tdP) ? tdP.GetString() : null;
        var prevFromDateStr = args.TryGetProperty("previousFromDate", out var pfdP) ? pfdP.GetString() : null;
        var prevToDateStr = args.TryGetProperty("previousToDate", out var ptdP) ? ptdP.GetString() : null;
        var categoryName = args.TryGetProperty("categoryName", out var cnP) ? cnP.GetString()?.Trim() : null;

        DateTime start1, end1, start2, end2;
        string title1, title2;

        if (period?.Equals("Custom", StringComparison.OrdinalIgnoreCase) == true &&
            !string.IsNullOrWhiteSpace(fromDateStr) && !string.IsNullOrWhiteSpace(toDateStr) &&
            !string.IsNullOrWhiteSpace(prevFromDateStr) && !string.IsNullOrWhiteSpace(prevToDateStr))
        {
            start1 = ParseDate(fromDateStr) ?? DateTime.UtcNow.AddMonths(-1);
            end1 = ParseDate(toDateStr) ?? DateTime.UtcNow;
            start2 = ParseDate(prevFromDateStr) ?? DateTime.UtcNow.AddMonths(-2);
            end2 = ParseDate(prevToDateStr) ?? DateTime.UtcNow.AddMonths(-1);
            title1 = $"{fromDateStr} تا {toDateStr}";
            title2 = $"{prevFromDateStr} تا {prevToDateStr}";
        }
        else
        {
            // Default: Month Over Month
            var (s1, e1, t1) = GetShamsiMonthRange(0);
            var (s2, e2, t2) = GetShamsiMonthRange(-1);
            start1 = s1;
            end1 = e1;
            title1 = t1;
            start2 = s2;
            end2 = e2;
            title2 = t2;
        }

        var request = new CategorySalesComparisonRpRequest(start1, end1, start2, end2, title1, title2, null, categoryName);
        var comparisons = await reportingService.GetCategorySalesComparisonAsync(request, ct);

        if (comparisons.Count == 0)
        {
            return McpContentResult.Text($"هیچ داده فروشی برای مقایسه بین دوره «{title1}» و «{title2}» یافت نشد.");
        }

        var totalW1 = comparisons.Sum(x => x.Weight1);
        var totalW2 = comparisons.Sum(x => x.Weight2);
        var totalWDelta = totalW2 > 0 ? ((totalW1 - totalW2) / totalW2) * 100m : (totalW1 > 0 ? 100m : 0m);

        var totalA1 = comparisons.Sum(x => x.Amount1);
        var totalA2 = comparisons.Sum(x => x.Amount2);
        var totalADelta = totalA2 > 0 ? ((totalA1 - totalA2) / totalA2) * 100m : (totalA1 > 0 ? 100m : 0m);

        string FormatDelta(decimal delta) => delta switch
        {
            > 0 => $"🟢 **+{delta:F1}%**",
            < 0 => $"🔴 **{delta:F1}%**",
            _ => "⚪ **0%**"
        };

        var sb = new StringBuilder();
        sb.AppendLine($"# 📊 مقایسه تحلیلی فروش دسته‌بندی‌ها: **{title1}** در مقایسه با **{title2}**");
        sb.AppendLine($"📈 **مجموع کل فروش:** {totalW1:N3} گرم (vs {totalW2:N3} گرم - {FormatDelta(totalWDelta)}) | {totalA1:N0} تومان (vs {totalA2:N0} تومان - {FormatDelta(totalADelta)})\n");

        sb.AppendLine("| دسته‌بندی کالا | وزن دوره ۱ (گرم) | وزن دوره ۲ (گرم) | تغییر وزن (Δ%) | تعداد ۱ | تعداد ۲ | مبلغ دوره ۱ (تومان) | مبلغ دوره ۲ (تومان) | تغییر مبلغ (Δ%) | وضعیت |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |");

        foreach (var c in comparisons)
        {
            var status = c.WeightDeltaPercent >= 0 ? "🟢 رشد وزنی" : "🔴 افت وزنی";
            sb.AppendLine($"| **{c.CategoryTitle}** | **{c.Weight1:N3}** | {c.Weight2:N3} | {FormatDelta(c.WeightDeltaPercent)} | {c.Quantity1} | {c.Quantity2} | **{c.Amount1:N0}** | {c.Amount2:N0} | {FormatDelta(c.AmountDeltaPercent)} | {status} |");
        }

        var highestGrowth = comparisons.Where(x => x.Weight2 > 0).OrderByDescending(x => x.WeightDeltaPercent).FirstOrDefault();
        var highestDecline = comparisons.Where(x => x.Weight2 > 0).OrderBy(x => x.WeightDeltaPercent).FirstOrDefault();

        sb.AppendLine("\n### 🔍 نکات تحلیلی مقایسه دسته‌ها:");
        if (highestGrowth != null && highestGrowth.WeightDeltaPercent > 0)
        {
            sb.AppendLine($"- 🚀 **بیشترین رشد وزنی:** دسته‌بندی **«{highestGrowth.CategoryTitle}»** با **+{highestGrowth.WeightDeltaPercent:F1}% رشد** نسبت به دوره قبل");
        }
        if (highestDecline != null && highestDecline.WeightDeltaPercent < 0)
        {
            sb.AppendLine($"- ⚠️ **بیشترین کاهش وزنی:** دسته‌بندی **«{highestDecline.CategoryTitle}»** با **{highestDecline.WeightDeltaPercent:F1}% افت** نسبت به دوره قبل");
        }

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteGetInvoiceDetailsAsync(JsonElement args, CancellationToken ct)
    {
        GetInvoiceResponse? invoice = null;

        if (args.TryGetProperty("invoiceId", out var idProp) && Guid.TryParse(idProp.GetString(), out var id))
        {
            invoice = await invoiceService.GetAsync(id, ct);
        }
        else if (args.TryGetProperty("invoiceNumber", out var numProp))
        {
            var num = numProp.GetInt64();
            var itStr = args.TryGetProperty("invoiceType", out var itP) ? itP.GetString() : "Sell";
            var it = Enum.TryParse<InvoiceType>(itStr, true, out var parsedIt) ? parsedIt : InvoiceType.Sell;
            invoice = await invoiceService.GetAsync(num, it, ct);
        }

        if (invoice == null)
            return McpContentResult.Text("فاکتور مورد نظر یافت نشد. لطفاً شماره فاکتور یا شناسه را بررسی فرمایید.", isError: true);

        var unitTitle = invoice.PriceUnit?.Title ?? "";
        var unpaidUnitTitle = invoice.UnpaidPriceUnit?.Title ?? unitTitle;
        var baseUrl = GetBaseUrl();
        var downloadUrl = $"{baseUrl}/api/invoices/{invoice.Id}/download-pdf";

        var sb = new StringBuilder();
        sb.AppendLine($"# 📄 جزئیات فاکتور شماره {invoice.InvoiceNumber} ({invoice.InvoiceType})");
        sb.AppendLine($"📥 **[دانلود فایل PDF فاکتور]({downloadUrl})**\n");

        // 1. Customer & General Info
        sb.AppendLine("### 👤 مشخصات مشتری و فاکتور:");
        sb.AppendLine($"- **مشتری:** {invoice.Customer?.FullName ?? "عمومی"}");
        if (!string.IsNullOrEmpty(invoice.Customer?.PhoneNumber))
            sb.AppendLine($"- **شماره همراه:** {invoice.Customer.PhoneNumber}");
        if (!string.IsNullOrEmpty(invoice.Customer?.NationalId))
            sb.AppendLine($"- **کد ملی:** {invoice.Customer.NationalId}");
        sb.AppendLine($"- **تاریخ فاکتور:** {invoice.InvoiceDate:yyyy/MM/dd}");
        if (invoice.DueDate.HasValue)
            sb.AppendLine($"- **تاریخ سررسید:** {invoice.DueDate.Value:yyyy/MM/dd}");
        sb.AppendLine($"- **نوع معامله:** {invoice.TradeScale}");

        // 2. Financial Summary
        sb.AppendLine("\n### 💰 خلاصه مالی فاکتور:");
        sb.AppendLine($"- **جمع کل ناخالص اقلام:** {invoice.TotalAmount:N0} {unitTitle}");
        if (invoice.TotalDiscountAmount > 0)
            sb.AppendLine($"- **تخفیف:** {invoice.TotalDiscountAmount:N0} {unitTitle}");
        if (invoice.TotalExtraCostAmount > 0)
            sb.AppendLine($"- **هزینه‌های اضافی:** {invoice.TotalExtraCostAmount:N0} {unitTitle}");
        sb.AppendLine($"- **مبلغ نهایی فاکتور:** **{invoice.TotalAmountWithDiscountsAndExtraCosts:N0} {unitTitle}**");
        sb.AppendLine($"- **مجموع پرداختی و تسویه‌ها:** {invoice.TotalPaidAmount:N0} {unitTitle}");
        sb.AppendLine($"- **مانده تسویه‌نشده:** **{invoice.TotalUnpaidAmount:N0} {unpaidUnitTitle}** " + (invoice.TotalUnpaidAmount <= 0 ? "🟢 (تسویه کامل)" : "🔴 (دارای مانده)"));

        // 3. Product Items (Gold & Jewelry)
        if (invoice.InvoiceProductItems?.Count > 0)
        {
            sb.AppendLine("\n### 💍 اقلام طلا و جواهر:");
            sb.AppendLine("| ردیف | شرح کالا | بارکد | وزن (گرم) | عیار | نرخ گرم | اجرت | سود | مالیات | مبلغ نهایی |");
            sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |");
            int idx = 1;
            foreach (var itm in invoice.InvoiceProductItems)
            {
                var wageStr = itm.SaleWageType == WageType.Percent ? $"{itm.SaleWage}%" : $"{itm.ItemWageAmount:N0}";
                var barcode = !string.IsNullOrEmpty(itm.Product?.Barcode) ? $"`{itm.Product.Barcode}`" : "-";
                sb.AppendLine($"| {idx++} | {itm.Product?.Name ?? "کالای طلا"} | {barcode} | {itm.TotalWeight:N3} | {itm.Product?.Fineness} | {itm.GramPrice:N0} | {wageStr} | {itm.ItemProfitAmount:N0} | {itm.ItemTaxAmount:N0} | **{itm.ItemFinalAmount:N0} {unitTitle}** |");
            }
        }

        // 4. Used Gold Items (Scrap Gold received)
        if (invoice.InvoiceUsedProducts?.Count > 0)
        {
            sb.AppendLine("\n### ♻️ اقلام طلای کهنه و متفرقه دریافتی:");
            sb.AppendLine("| ردیف | شرح کالا | وزن (گرم) | عیار | کسری از ۷۵۰ | نرخ گرم | وضعیت | ارزش کل |");
            sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |");
            int idx = 1;
            foreach (var ug in invoice.InvoiceUsedProducts)
            {
                var status = ug.IsBroken ? "شکسته" : "سالم";
                sb.AppendLine($"| {idx++} | {ug.Description} | {ug.Weight:N3} | {ug.Fineness} | {ug.FinenessDeductionRate} | {ug.GramPrice:N0} | {status} | **{ug.ItemAmount:N0} {unitTitle}** |");
            }
        }

        // 5. Coin Items
        if (invoice.InvoiceCoinItems?.Count > 0)
        {
            sb.AppendLine("\n### 🪙 اقلام سکه:");
            sb.AppendLine("| ردیف | عنوان سکه | تعداد | قیمت واحد | سود | مبلغ کل |");
            sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- |");
            int idx = 1;
            foreach (var c in invoice.InvoiceCoinItems)
            {
                var total = c.Quantity * c.UnitPrice;
                var coinTitle = c.Coin?.Coin?.Title ?? "سکه";
                sb.AppendLine($"| {idx++} | {coinTitle} | {c.Quantity} | {c.UnitPrice:N0} | {c.ProfitPercent}% | **{total:N0} {unitTitle}** |");
            }
        }

        // 6. Currency Items
        if (invoice.InvoiceCurrencyItems?.Count > 0)
        {
            sb.AppendLine("\n### 💵 اقلام ارزی:");
            sb.AppendLine("| ردیف | ارز | مقدار | نرخ تبدیل | مبلغ کل |");
            sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");
            int idx = 1;
            foreach (var cur in invoice.InvoiceCurrencyItems)
            {
                var total = cur.Amount * cur.UnitPrice;
                sb.AppendLine($"| {idx++} | {cur.Currency?.Title ?? "ارز"} | {cur.Amount:N2} | {cur.UnitPrice:N0} | **{total:N0} {unitTitle}** |");
            }
        }

        // 7. Payments Breakdown
        if (invoice.InvoicePayments?.Count > 0)
        {
            sb.AppendLine("\n### 💳 ریز دریافتی‌ها و پرداختی‌های فاکتور:");
            sb.AppendLine("| ردیف | نوع پرداخت | حساب مالی / بانک | مبلغ / مقدار | واحد | شماره پیگیری / ارجاع | توضیحات |");
            sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- |");
            int idx = 1;
            foreach (var p in invoice.InvoicePayments)
            {
                var pTypeStr = p.PaymentType.GetDisplayTitle();
                var accTitle = p.FinancialAccount?.Title ?? "-";
                var checkDetails = p.CheckPayment != null
                    ? $"(چک صیادی: {p.CheckPayment.SayadiCode} - سررسید: {p.CheckPayment.DueDate:yyyy/MM/dd})"
                    : "";
                var noteStr = $"{p.Note ?? ""} {checkDetails}".Trim();

                sb.AppendLine($"| {idx++} | {pTypeStr} | {accTitle} | **{p.Amount:N0}** | {p.PriceUnit?.Title} | {p.ReferenceNumber ?? "-"} | {noteStr} |");
            }
        }

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteCreateInvoiceAsync(JsonElement args, CancellationToken ct)
    {
        // 1. Resolve Invoice Type
        var invoiceTypeStr = args.TryGetProperty("invoiceType", out var itP) ? itP.GetString() : "Sell";
        var invoiceType = Enum.TryParse<InvoiceType>(invoiceTypeStr, true, out var it) ? it : InvoiceType.Sell;

        // 2. Resolve Customer
        Guid? customerId = null;
        string? customerFullName = null;

        if (args.TryGetProperty("customerId", out var cidP) && Guid.TryParse(cidP.GetString(), out var parsedCid))
        {
            var cust = await customerService.GetAsync(parsedCid, ct);
            if (cust != null)
            {
                customerId = cust.Id;
                customerFullName = cust.FullName;
            }
        }

        if (!customerId.HasValue && args.TryGetProperty("customerName", out var cnP) && !string.IsNullOrWhiteSpace(cnP.GetString()))
        {
            var searchName = cnP.GetString()!.Trim();
            var matches = await customerService.GetByNameAsync(searchName, null, ct);
            if (matches.Count > 0)
            {
                var first = matches.First();
                customerId = first.Id;
                customerFullName = first.FullName;
            }
            else
            {
                // Auto-create customer if not found
                var phone = args.TryGetProperty("customerPhone", out var cpP) ? cpP.GetString() : "";
                var genNat = await customerService.GenerateNationalIdAsync(ct);
                var defaultPriceUnit = await priceUnitService.GetDefaultAsync(ct);

                var newCustRequest = new CustomerRequestDto(
                    Id: null,
                    FullName: searchName,
                    NationalId: genNat.NationalId,
                    PhoneNumber: phone ?? "",
                    Address: null,
                    CreditLimit: null,
                    CreditLimitPriceUnitId: defaultPriceUnit?.Id,
                    CustomerType: CustomerType.RetailCustomer);

                customerId = await customerService.CreateAsync(newCustRequest, ct);
                customerFullName = searchName;
            }
        }

        if (!customerId.HasValue)
        {
            return McpContentResult.Text("❌ مشتری برای ثبت فاکتور مشخص نشده یا یافت نشد. لطفاً نام مشتری (customerName) یا شناسه (customerId) را ارسال فرمایید.", isError: true);
        }

        // 3. Resolve PriceUnit
        var defaultUnit = await priceUnitService.GetDefaultAsync(ct);
        var priceUnitId = defaultUnit?.Id ?? Guid.Empty;

        // 4. Resolve Live Gold Rate (if needed for items with 0 rate)
        decimal live18KRate = 0;
        try
        {
            var prices = await priceService.GetListAsync(null, ct);
            var gold18Price = prices.FirstOrDefault(x => x.Title.Contains("18") || x.Title.Contains("طلا"));
            if (gold18Price != null && decimal.TryParse(gold18Price.Value?.Replace(",", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedVal))
            {
                live18KRate = parsedVal;
            }
        }
        catch
        {
            // Fallback
        }

        // 5. Build Product Items
        var productItemDtos = new List<InvoiceProductItemDto>();
        if (args.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var itemElem in itemsProp.EnumerateArray())
            {
                var itemName = itemElem.TryGetProperty("name", out var nP) ? nP.GetString() : "کالای طلا";
                var weight = itemElem.TryGetProperty("weight", out var wP) ? wP.GetDecimal() : 0m;
                if (weight <= 0) continue;

                var gramPrice = itemElem.TryGetProperty("gramPrice", out var gpP) && gpP.GetDecimal() > 0
                    ? gpP.GetDecimal()
                    : live18KRate;

                if (gramPrice <= 0)
                {
                    return McpContentResult.Text("❌ نرخ هر گرم طلا مشخص نشده و نرخ زنده بازار در دسترس نیست. لطفاً نرخ گرمی طلا را اعلام فرمایید.", isError: true);
                }

                var fineness = itemElem.TryGetProperty("fineness", out var fP) ? fP.GetDecimal() : 750m;
                var wageType = itemElem.TryGetProperty("wageType", out var wtP) && wtP.GetString()?.Equals("Fixed", StringComparison.OrdinalIgnoreCase) == true
                    ? WageType.Fixed
                    : WageType.Percent;
                var wage = itemElem.TryGetProperty("wage", out var wgP) ? wgP.GetDecimal() : 0m;
                var profitPercent = itemElem.TryGetProperty("profitPercent", out var ppP) ? ppP.GetDecimal() : 7m;
                var taxPercent = itemElem.TryGetProperty("taxPercent", out var tpP) ? tpP.GetDecimal() : 9m;
                var quantity = itemElem.TryGetProperty("quantity", out var qP) ? qP.GetInt32() : 1;

                var prodDto = new ProductRequestDto(
                    Id: null,
                    Name: itemName,
                    Barcode: null,
                    Weight: weight,
                    Wage: wage,
                    WageType: wageType,
                    ProductType: ProductType.Gold,
                    Fineness: fineness,
                    GoldUnitType: GoldUnitType.Gram,
                    ProductCategoryId: null,
                    WagePriceUnitId: priceUnitId,
                    StonePriceUnitId: null,
                    GemStones: null,
                    MoltenGold: null);

                productItemDtos.Add(new InvoiceProductItemDto(
                    Id: null,
                    GramPrice: gramPrice,
                    ProfitPercent: profitPercent,
                    TaxPercent: taxPercent,
                    CostPrice: null,
                    CostPriceExchangeRate: null,
                    WagePriceUnitExchangeRate: null,
                    StonePriceUnitExchangeRate: null,
                    CostPriceUnitId: null,
                    IsInstantProduct: false,
                    Quantity: quantity,
                    TotalWeight: weight,
                    PurchaseWage: null,
                    PurchaseWageType: null,
                    Product: prodDto));
            }
        }

        // 6. Build Used Gold Items (Scrap gold)
        var usedGoldDtos = new List<InvoiceUsedProductDto>();
        if (args.TryGetProperty("usedGoldItems", out var usedProp) && usedProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var uElem in usedProp.EnumerateArray())
            {
                var desc = uElem.TryGetProperty("description", out var dP) ? dP.GetString() : "طلای کهنه";
                var weight = uElem.TryGetProperty("weight", out var wP) ? wP.GetDecimal() : 0m;
                if (weight <= 0) continue;

                var fineness = uElem.TryGetProperty("fineness", out var fP) ? fP.GetDecimal() : 750m;
                var deduction = uElem.TryGetProperty("deductionFrom750", out var dedP) ? dedP.GetDecimal() : 0m;
                var gramPrice = uElem.TryGetProperty("gramPrice", out var gpP) && gpP.GetDecimal() > 0
                    ? gpP.GetDecimal()
                    : live18KRate;
                var isBroken = uElem.TryGetProperty("isBroken", out var bP) && bP.GetBoolean();

                usedGoldDtos.Add(new InvoiceUsedProductDto(
                    Id: null,
                    Description: desc ?? "طلای کهنه",
                    Weight: weight,
                    GramPrice: gramPrice,
                    ExtraCostsAmount: null,
                    Fineness: fineness,
                    FinenessDeductionRate: deduction,
                    Quantity: 1,
                    IsBroken: isBroken,
                    ProductType: ProductType.UsedGold,
                    UnitType: GoldUnitType.Gram));
            }
        }

        // 7. Resolve Financial Accounts & Build Payments
        var accountTitles = await financialAccountService.GetTitlesAsync(null, null, ct);
        var accounts = await financialAccountService.GetAllAsync(ct);
        var defaultCashAccount = accounts.FirstOrDefault(x => x.FinancialAccountType == FinancialAccountType.Cash);
        var defaultBankAcc = accounts.FirstOrDefault(x => x.FinancialAccountType == FinancialAccountType.LocalBankAccount);

        var paymentDtos = new List<InvoicePaymentDto>();
        if (args.TryGetProperty("payments", out var payProp) && payProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var pElem in payProp.EnumerateArray())
            {
                var amount = pElem.TryGetProperty("amount", out var aP) ? aP.GetDecimal() : 0m;
                if (amount <= 0) continue;

                var pTypeStr = pElem.TryGetProperty("paymentType", out var ptP) ? ptP.GetString() : "InternalCash";
                var pType = PaymentType.InternalCash;
                if (pTypeStr?.Equals("Check", StringComparison.OrdinalIgnoreCase) == true)
                    pType = PaymentType.Check;
                else if (pTypeStr?.Equals("UsedGold", StringComparison.OrdinalIgnoreCase) == true)
                    pType = PaymentType.UsedGoldInventory;
                else if (pTypeStr?.Equals("CustomerTransfer", StringComparison.OrdinalIgnoreCase) == true)
                    pType = PaymentType.CustomerTransfer;

                Guid? targetAccId = null;
                if (pElem.TryGetProperty("financialAccountTitle", out var accTitleP) && !string.IsNullOrWhiteSpace(accTitleP.GetString()))
                {
                    var match = accountTitles.FirstOrDefault(x => x.Title.Contains(accTitleP.GetString()!, StringComparison.OrdinalIgnoreCase));
                    if (match != null) targetAccId = match.Id;
                }

                targetAccId ??= defaultCashAccount?.Id ?? defaultBankAcc?.Id ?? accounts.FirstOrDefault()?.Id;

                var refNum = pElem.TryGetProperty("referenceNumber", out var rfP) ? rfP.GetString() : null;
                var note = pElem.TryGetProperty("note", out var ntP) ? ntP.GetString() : "دریافتی فاکتور";

                // Check payments
                var checkNum = pElem.TryGetProperty("checkNumber", out var chkNP) ? chkNP.GetString() : null;
                var checkSayadi = pElem.TryGetProperty("checkSayadiCode", out var chkSP) ? chkSP.GetString() : null;
                DateTime? checkDueDate = null;
                if (pElem.TryGetProperty("checkDueDate", out var chkDP) && DateTime.TryParse(chkDP.GetString(), out var parsedCd))
                {
                    checkDueDate = parsedCd;
                }

                paymentDtos.Add(new InvoicePaymentDto(
                    Id: null,
                    Amount: amount,
                    ExchangeRate: null,
                    GoldFineness: null,
                    PaymentType: pType,
                    PaymentSide: PaymentSide.Receive,
                    PaymentDate: DateTime.UtcNow,
                    ReferenceNumber: refNum,
                    Note: note,
                    FinancialAccountId: targetAccId,
                    VoucherId: null,
                    TargetInvoiceId: null,
                    CustomerId: customerId.Value,
                    PriceUnitId: priceUnitId,
                    CheckIssuerId: null,
                    CheckIssuerFinancialAccountId: null,
                    CheckNumber: checkNum,
                    CheckSayadiCode: checkSayadi,
                    CheckDueDate: checkDueDate,
                    CheckImage: null,
                    CheckImageContentType: null));
            }
        }

        // 8. Discounts & Extra Costs
        var discountDtos = new List<InvoiceDiscountDto>();
        if (args.TryGetProperty("discounts", out var discProp) && discProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var dElem in discProp.EnumerateArray())
            {
                var amount = dElem.TryGetProperty("amount", out var aP) ? aP.GetDecimal() : 0m;
                var desc = dElem.TryGetProperty("description", out var dP) ? dP.GetString() : "تخفیف";
                if (amount > 0)
                {
                    discountDtos.Add(new InvoiceDiscountDto(amount, null, desc, priceUnitId));
                }
            }
        }

        var extraCostsDtos = new List<InvoiceExtraCostsDto>();
        if (args.TryGetProperty("extraCosts", out var ecProp) && ecProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var ecElem in ecProp.EnumerateArray())
            {
                var amount = ecElem.TryGetProperty("amount", out var aP) ? aP.GetDecimal() : 0m;
                var desc = ecElem.TryGetProperty("description", out var dP) ? dP.GetString() : "هزینه اضافی";
                if (amount > 0)
                {
                    extraCostsDtos.Add(new InvoiceExtraCostsDto(amount, null, desc, priceUnitId));
                }
            }
        }

        if (productItemDtos.Count == 0 && usedGoldDtos.Count == 0)
        {
            return McpContentResult.Text("❌ حداقل یک ردیف کالا (طلا یا طلای کهنه) برای ثبت فاکتور الزامی است.", isError: true);
        }

        // 9. Generate Invoice Number & Create Invoice Request
        var lastNumRes = await invoiceService.GetLastNumberAsync(invoiceType, ct);
        var newInvoiceNumber = lastNumRes.InvoiceNumber + 1;

        var invoiceRequest = new InvoiceRequestDto(
            Id: null,
            InvoiceNumber: newInvoiceNumber,
            InvoiceDate: DateTime.UtcNow,
            DueDate: null,
            InvoiceType: invoiceType,
            TradeScale: TradeScale.Retail,
            PriceUnitId: priceUnitId,
            UnpaidAmountExchangeRate: null,
            UnpaidPriceUnitId: null,
            ExchangeRate: null,
            CustomerId: customerId.Value,
            InvoiceProductItems: productItemDtos,
            InvoiceCoinItems: [],
            InvoiceCurrencyItems: [],
            InvoiceDiscounts: discountDtos,
            InvoicePayments: paymentDtos,
            InvoiceExtraCosts: extraCostsDtos,
            InvoiceUsedProducts: usedGoldDtos);

        // 10. Execute Transactional Creation in Application Layer
        await invoiceService.CreateAsync(invoiceRequest, ct);

        // 11. Retrieve Created Invoice for rich report
        var createdInvoice = await invoiceService.GetAsync(newInvoiceNumber, invoiceType, ct);
        var baseUrl = GetBaseUrl();
        var downloadUrl = $"{baseUrl}/api/invoices/{createdInvoice.Id}/download-pdf";

        var unitTitle = createdInvoice.PriceUnit?.Title ?? "";
        var sb = new StringBuilder();
        sb.AppendLine($"# 🎉 فاکتور شماره {createdInvoice.InvoiceNumber} با موفقیت ثبت شد!");
        sb.AppendLine($"📥 **[دانلود مستقیم فایل PDF فاکتور]({downloadUrl})**\n");
        sb.AppendLine("### 📋 خلاصه فاکتور صادرشده:");
        sb.AppendLine($"- **مشتری:** {customerFullName}");
        sb.AppendLine($"- **نوع فاکتور:** {createdInvoice.InvoiceType}");
        sb.AppendLine($"- **تاریخ ثبت:** {createdInvoice.InvoiceDate:yyyy/MM/dd}");
        sb.AppendLine($"- **مبلغ نهایی فاکتور:** **{createdInvoice.TotalAmountWithDiscountsAndExtraCosts:N0} {unitTitle}**");
        sb.AppendLine($"- **مجموع دریافتی‌ها:** {createdInvoice.TotalPaidAmount:N0} {unitTitle}");
        sb.AppendLine($"- **مانده تسویه‌نشده:** **{createdInvoice.TotalUnpaidAmount:N0} {unitTitle}** " + (createdInvoice.TotalUnpaidAmount <= 0 ? "🟢 (تسویه کامل)" : "🔴 (دارای مانده)"));

        if (createdInvoice.InvoiceProductItems?.Count > 0)
        {
            sb.AppendLine("\n**اقلام طلا:**");
            foreach (var itm in createdInvoice.InvoiceProductItems)
            {
                sb.AppendLine($"- {itm.Product?.Name}: وزن {itm.TotalWeight:N3} گرم - عیار {itm.Product?.Fineness} - مبلغ: {itm.ItemFinalAmount:N0} {unitTitle}");
            }
        }

        if (createdInvoice.InvoiceUsedProducts?.Count > 0)
        {
            sb.AppendLine("\n**اقلام طلای کهنه دریافتی:**");
            foreach (var ug in createdInvoice.InvoiceUsedProducts)
            {
                sb.AppendLine($"- {ug.Description}: وزن {ug.Weight:N3} گرم - کسری {ug.FinenessDeductionRate} - ارزش: {ug.ItemAmount:N0} {unitTitle}");
            }
        }

        if (createdInvoice.InvoicePayments?.Count > 0)
        {
            sb.AppendLine("\n**دریافتی‌های ثبت‌شده:**");
            foreach (var p in createdInvoice.InvoicePayments)
            {
                sb.AppendLine($"- {p.PaymentType.GetDisplayTitle()}: {p.Amount:N0} {unitTitle} ({p.FinancialAccount?.Title ?? "صندوق"})");
            }
        }

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteAddInvoicePaymentAsync(JsonElement args, CancellationToken ct)
    {
        if (!args.TryGetProperty("amount", out var aProp) || aProp.GetDecimal() <= 0)
        {
            return McpContentResult.Text("مبلغ پرداختی (amount) الزامی و باید بیشتر از صفر باشد.", isError: true);
        }

        var amount = aProp.GetDecimal();
        GetInvoiceResponse? existingInvoice = null;

        if (args.TryGetProperty("invoiceId", out var idProp) && Guid.TryParse(idProp.GetString(), out var id))
        {
            existingInvoice = await invoiceService.GetAsync(id, ct);
        }
        else if (args.TryGetProperty("invoiceNumber", out var numProp))
        {
            var num = numProp.GetInt64();
            var itStr = args.TryGetProperty("invoiceType", out var itP) ? itP.GetString() : "Sell";
            var it = Enum.TryParse<InvoiceType>(itStr, true, out var parsedIt) ? parsedIt : InvoiceType.Sell;
            existingInvoice = await invoiceService.GetAsync(num, it, ct);
        }

        if (existingInvoice == null)
        {
            return McpContentResult.Text("❌ فاکتور مورد نظر برای افزودن پرداخت یافت نشد. لطفاً شماره فاکتور یا شناسه را بررسی فرمایید.", isError: true);
        }

        var pTypeStr = args.TryGetProperty("paymentType", out var ptProp) ? ptProp.GetString() : "InternalCash";
        var pType = PaymentType.InternalCash;
        if (pTypeStr?.Equals("Check", StringComparison.OrdinalIgnoreCase) == true)
            pType = PaymentType.Check;
        else if (pTypeStr?.Equals("UsedGold", StringComparison.OrdinalIgnoreCase) == true)
            pType = PaymentType.UsedGoldInventory;
        else if (pTypeStr?.Equals("CustomerTransfer", StringComparison.OrdinalIgnoreCase) == true)
            pType = PaymentType.CustomerTransfer;

        var accountTitles = await financialAccountService.GetTitlesAsync(null, null, ct);
        var accounts = await financialAccountService.GetAllAsync(ct);
        var defaultCashAccount = accounts.FirstOrDefault(x => x.FinancialAccountType == FinancialAccountType.Cash);
        var defaultBankAcc = accounts.FirstOrDefault(x => x.FinancialAccountType == FinancialAccountType.LocalBankAccount);

        Guid? targetAccId = null;
        if (args.TryGetProperty("financialAccountTitle", out var accTitleP) && !string.IsNullOrWhiteSpace(accTitleP.GetString()))
        {
            var match = accountTitles.FirstOrDefault(x => x.Title.Contains(accTitleP.GetString()!, StringComparison.OrdinalIgnoreCase));
            if (match != null) targetAccId = match.Id;
        }

        targetAccId ??= defaultCashAccount?.Id ?? defaultBankAcc?.Id ?? accounts.FirstOrDefault()?.Id;

        var refNum = args.TryGetProperty("referenceNumber", out var rfP) ? rfP.GetString() : null;
        var note = args.TryGetProperty("note", out var ntP) ? ntP.GetString() : "پرداخت ثبت‌شده از طریق هوش مصنوعی";

        var checkNum = args.TryGetProperty("checkNumber", out var chkNP) ? chkNP.GetString() : null;
        var checkSayadi = args.TryGetProperty("checkSayadiCode", out var chkSP) ? chkSP.GetString() : null;
        DateTime? checkDueDate = null;
        if (args.TryGetProperty("checkDueDate", out var chkDP) && DateTime.TryParse(chkDP.GetString(), out var parsedCd))
        {
            checkDueDate = parsedCd;
        }

        // Map existing invoice to request DTO
        var requestDto = MapToRequestDto(existingInvoice);

        // Append new payment
        var newPaymentDto = new InvoicePaymentDto(
            Id: null,
            Amount: amount,
            ExchangeRate: null,
            GoldFineness: null,
            PaymentType: pType,
            PaymentSide: PaymentSide.Receive,
            PaymentDate: DateTime.UtcNow,
            ReferenceNumber: refNum,
            Note: note,
            FinancialAccountId: targetAccId,
            VoucherId: null,
            TargetInvoiceId: null,
            CustomerId: existingInvoice.Customer.Id,
            PriceUnitId: existingInvoice.PriceUnit.Id,
            CheckIssuerId: null,
            CheckIssuerFinancialAccountId: null,
            CheckNumber: checkNum,
            CheckSayadiCode: checkSayadi,
            CheckDueDate: checkDueDate,
            CheckImage: null,
            CheckImageContentType: null);

        requestDto.InvoicePayments.Add(newPaymentDto);

        // Update Invoice
        await invoiceService.UpdateAsync(existingInvoice.Id, requestDto, ct);

        // Fetch updated invoice
        var updatedInvoice = await invoiceService.GetAsync(existingInvoice.Id, ct);
        var baseUrl = GetBaseUrl();
        var downloadUrl = $"{baseUrl}/api/invoices/{updatedInvoice.Id}/download-pdf";
        var unitTitle = updatedInvoice.PriceUnit?.Title ?? "";

        var sb = new StringBuilder();
        sb.AppendLine($"# ✅ پرداخت به مبلغ {amount:N0} {unitTitle} در فاکتور شماره {updatedInvoice.InvoiceNumber} با موفقیت ثبت شد!");
        sb.AppendLine($"📥 **[دانلود نسخه به‌روزشده فایل PDF فاکتور]({downloadUrl})**\n");
        sb.AppendLine($"- **مشتری:** {updatedInvoice.Customer?.FullName}");
        sb.AppendLine($"- **نوع پرداخت:** {pType.GetDisplayTitle()}");
        sb.AppendLine($"- **مبلغ کل فاکتور:** {updatedInvoice.TotalAmountWithDiscountsAndExtraCosts:N0} {unitTitle}");
        sb.AppendLine($"- **مجموع پرداختی‌های قبلی و جدید:** {updatedInvoice.TotalPaidAmount:N0} {unitTitle}");
        sb.AppendLine($"- **مانده تسویه‌نشده جدید:** **{updatedInvoice.TotalUnpaidAmount:N0} {unitTitle}** " + (updatedInvoice.TotalUnpaidAmount <= 0 ? "🟢 (تسویه کامل)" : "🔴 (دارای مانده)"));

        return McpContentResult.Text(sb.ToString());
    }

    private static InvoiceRequestDto MapToRequestDto(GetInvoiceResponse invoice)
    {
        var productItems = invoice.InvoiceProductItems?.Select(p => new InvoiceProductItemDto(
            Id: p.Id,
            GramPrice: p.GramPrice,
            ProfitPercent: p.ProfitPercent,
            TaxPercent: p.TaxPercent,
            CostPrice: p.CostPrice,
            CostPriceExchangeRate: p.CostPriceExchangeRate,
            WagePriceUnitExchangeRate: p.WageExchangeRate,
            StonePriceUnitExchangeRate: p.StonePriceUnitExchangeRate,
            CostPriceUnitId: p.CostPriceUnitId,
            IsInstantProduct: p.IsInstantProduct,
            Quantity: p.Quantity,
            TotalWeight: p.TotalWeight,
            PurchaseWage: p.PurchaseWage,
            PurchaseWageType: p.PurchaseWageType,
            Product: new ProductRequestDto(
                Id: p.Product?.Id,
                Name: p.Product?.Name,
                Barcode: p.Product?.Barcode,
                Weight: p.Product?.Weight ?? p.TotalWeight,
                Wage: p.Product?.Wage ?? 0,
                WageType: p.Product?.WageType,
                ProductType: p.Product?.ProductType ?? ProductType.Gold,
                Fineness: p.Product?.Fineness ?? 750,
                GoldUnitType: p.Product?.GoldUnitType ?? GoldUnitType.Gram,
                ProductCategoryId: p.Product?.ProductCategoryId,
                WagePriceUnitId: p.Product?.WagePriceUnitId,
                StonePriceUnitId: p.Product?.StonePriceUnit?.Id,
                GemStones: null,
                MoltenGold: null)
        )).ToList() ?? [];

        var usedProducts = invoice.InvoiceUsedProducts?.Select(u => new InvoiceUsedProductDto(
            Id: u.Id,
            Description: u.Description,
            Weight: u.Weight,
            GramPrice: u.GramPrice,
            ExtraCostsAmount: u.ExtraCostsAmount,
            Fineness: u.Fineness,
            FinenessDeductionRate: u.FinenessDeductionRate,
            Quantity: 1,
            IsBroken: u.IsBroken,
            ProductType: u.ProductType,
            UnitType: u.UnitType
        )).ToList() ?? [];

        var coinItems = invoice.InvoiceCoinItems?.Select(c => new InvoiceCoinItemDto(
            Id: c.Id,
            UnitPrice: c.UnitPrice,
            Quantity: c.Quantity,
            ProfitPercent: c.ProfitPercent,
            IsInstant: c.IsInstant,
            CoinInstance: new CoinInstanceRequestDto(
                Id: c.Coin?.Id,
                CoinId: c.Coin?.Coin?.Id ?? Guid.Empty,
                Barcode: c.Coin?.Barcode,
                MintYear: c.Coin?.MintYear,
                Weight: c.Coin?.Weight ?? 0,
                Fineness: c.Coin?.Fineness ?? 900,
                MintType: c.Coin?.MintType ?? CoinMintType.Banking,
                PackageType: c.Coin?.PackageType ?? CoinPackageType.VacuumSealed,
                CoinPackage: null)
        )).ToList() ?? [];

        var currencyItems = invoice.InvoiceCurrencyItems?.Select(cur => new InvoiceCurrencyItemDto(
            Id: cur.Id,
            UnitPrice: cur.UnitPrice,
            Amount: cur.Amount,
            ProfitPercent: cur.ProfitPercent,
            TaxPercent: cur.TaxPercent,
            CurrencyId: cur.Currency.Id,
            FinancialAccountId: cur.FinancialAccount?.Id ?? Guid.Empty
        )).ToList() ?? [];

        var discounts = invoice.InvoiceDiscounts?.Select(d => new InvoiceDiscountDto(
            Amount: d.Amount,
            ExchangeRate: d.ExchangeRate,
            Description: d.Description,
            PriceUnitId: d.PriceUnit?.Id ?? invoice.PriceUnit.Id
        )).ToList() ?? [];

        var extraCosts = invoice.InvoiceExtraCosts?.Select(ec => new InvoiceExtraCostsDto(
            Amount: ec.Amount,
            ExchangeRate: ec.ExchangeRate,
            Description: ec.Description,
            PriceUnitId: ec.PriceUnit?.Id ?? invoice.PriceUnit.Id
        )).ToList() ?? [];

        var payments = invoice.InvoicePayments?.Select(pay => new InvoicePaymentDto(
            Id: pay.Id,
            Amount: pay.Amount,
            ExchangeRate: pay.ExchangeRate,
            GoldFineness: pay.GoldFineness,
            PaymentType: pay.PaymentType,
            PaymentSide: pay.PaymentSide,
            PaymentDate: pay.PaymentDate,
            ReferenceNumber: pay.ReferenceNumber,
            Note: pay.Note,
            FinancialAccountId: pay.FinancialAccount?.Id,
            VoucherId: pay.VoucherId,
            TargetInvoiceId: pay.TargetInvoice?.Id,
            CustomerId: invoice.Customer.Id,
            PriceUnitId: pay.PriceUnit.Id,
            CheckIssuerId: pay.Endorser?.Id,
            CheckIssuerFinancialAccountId: null,
            CheckNumber: pay.CheckPayment?.Number,
            CheckSayadiCode: pay.CheckPayment?.SayadiCode,
            CheckDueDate: pay.CheckPayment?.DueDate,
            CheckImage: null,
            CheckImageContentType: null
        )).ToList() ?? [];

        return new InvoiceRequestDto(
            Id: invoice.Id,
            InvoiceNumber: invoice.InvoiceNumber,
            InvoiceDate: invoice.InvoiceDate,
            DueDate: invoice.DueDate,
            InvoiceType: invoice.InvoiceType,
            TradeScale: invoice.TradeScale,
            PriceUnitId: invoice.PriceUnit.Id,
            UnpaidAmountExchangeRate: invoice.UnpaidAmountExchangeRate,
            UnpaidPriceUnitId: invoice.UnpaidPriceUnit?.Id,
            ExchangeRate: invoice.ExchangeRate,
            CustomerId: invoice.Customer.Id,
            InvoiceProductItems: productItems,
            InvoiceCoinItems: coinItems,
            InvoiceCurrencyItems: currencyItems,
            InvoiceDiscounts: discounts,
            InvoicePayments: payments,
            InvoiceExtraCosts: extraCosts,
            InvoiceUsedProducts: usedProducts);
    }

    private async Task<McpContentResult> ExecuteGetTrialBalanceReportAsync(JsonElement args, CancellationToken ct)
    {
        var fromDateStr = args.TryGetProperty("fromDate", out var fdP) ? fdP.GetString() : null;
        var toDateStr = args.TryGetProperty("toDate", out var tdP) ? tdP.GetString() : null;
        var start = ParseDate(fromDateStr);
        var end = ParseDate(toDateStr);

        var request = new LedgerAccountTrialBalanceRpRequest(null, start, end);
        var report = await reportingService.GetLedgerAccountTrialBalanceAsync(request, ct);

        if (report.Count == 0)
            return McpContentResult.Text("داده‌ای برای تراز آزمایشی در این بازه یافت نشد.");

        var sb = new StringBuilder();
        sb.AppendLine($"### ⚖️ تراز آزمایشی حسابداری ({report.Count} سرفصل):");
        sb.AppendLine("| عنوان حساب | نوع حساب | بدهکار | بستانکار | واحد مبنا |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");

        foreach (var r in report.Take(50))
        {
            sb.AppendLine($"| {r.LedgerAccountTitle} | {r.LedgerAccountType} | {r.DebitAmountBase:N0} | {r.CreditAmountBase:N0} | {r.BasePriceUnitTitle} |");
        }

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteGetUsedGoldHiddenProfitAsync(JsonElement args, CancellationToken ct)
    {
        var fromDateStr = args.TryGetProperty("fromDate", out var fdP) ? fdP.GetString() : null;
        var toDateStr = args.TryGetProperty("toDate", out var tdP) ? tdP.GetString() : null;
        var start = ParseDate(fromDateStr);
        var end = ParseDate(toDateStr);

        var request = new UsedGoldHiddenProfitRpRequest(null, null, start, end);
        var report = await reportingService.GetUsedGoldHiddenProfitAsync(request, ct);

        if (report.Count == 0)
            return McpContentResult.Text("هیچ رکورد سود پنهانی در بازه زمانی تعیین‌شده یافت نشد.");

        var sb = new StringBuilder();
        sb.AppendLine("### 💰 گزارش سود پنهان ری‌گیری و آب کردن طلای کهنه:");
        sb.AppendLine("| شماره فاکتور | تاریخ | نام مشتری | وزن (گرم) | عیار اسمی | مبلغ پرداختی | ارزش واقعی | سود پنهان | واحد |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |");

        foreach (var r in report)
        {
            sb.AppendLine($"| {r.InvoiceNumber} | {r.InvoiceDate} | {r.CustomerName} | {r.Weight:N3} | {r.Fineness} | {r.PaidAmount:N0} | {r.RealValue:N0} | {r.HiddenProfit:N0} | {r.PriceUnitTitle} |");
        }

        return McpContentResult.Text(sb.ToString());
    }
}
