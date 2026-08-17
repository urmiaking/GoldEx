using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
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
    IStoreContext storeContext)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

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
                Name = "get_customer_statement",
                Description = "دریافت صورتحساب و ریز تراکنش‌های بدهکار/بستانکار دفتر کل یک مشتری در بازه زمانی مشخص",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["customerId"] = new() { Type = "string", Description = "شناسه مشتری" },
                        ["customerName"] = new() { Type = "string", Description = "نام مشتری در صورت نداشتن شناسه" },
                        ["fromDate"] = new() { Type = "string", Description = "تاریخ شروع (شمسی مثلاً 1403/01/01 یا میلادی 2024-03-21)" },
                        ["toDate"] = new() { Type = "string", Description = "تاریخ پایان" }
                    }
                }
            },
            new McpToolDefinition
            {
                Name = "search_invoices",
                Description = "جستجو در فاکتورهای ثبت‌شده فروش، خرید و مرجوعی فروشگاه",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["invoiceType"] = new() { Type = "string", Description = "نوع فاکتور (Sell برای فروش، Purchase برای خرید، Return برای مرجوعی)", Enum = ["Sell", "Purchase", "Return"] },
                        ["query"] = new() { Type = "string", Description = "شماره فاکتور یا نام مشتری" },
                        ["fromDate"] = new() { Type = "string", Description = "تاریخ شروع" },
                        ["toDate"] = new() { Type = "string", Description = "تاریخ پایان" }
                    }
                }
            },
            new McpToolDefinition
            {
                Name = "get_invoice_details",
                Description = "دریافت جزئیات کامل یک فاکتور شامل اقلام، اجرت‌ها، مالیات، دریافتی‌ها و معادل وزنی طلای ۱۸ عیار",
                InputSchema = new McpInputSchema
                {
                    Properties = new Dictionary<string, McpPropertySchema>
                    {
                        ["invoiceId"] = new() { Type = "string", Description = "شناسه یکتای فاکتور" },
                        ["invoiceNumber"] = new() { Type = "number", Description = "شماره فاکتور" },
                        ["invoiceType"] = new() { Type = "string", Description = "نوع فاکتور", Enum = ["Sell", "Purchase", "Return"] }
                    }
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
                        ["fromDate"] = new() { Type = "string", Description = "از تاریخ" },
                        ["toDate"] = new() { Type = "string", Description = "تا تاریخ" }
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
                        ["fromDate"] = new() { Type = "string", Description = "از تاریخ" },
                        ["toDate"] = new() { Type = "string", Description = "تا تاریخ" }
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
                "get_customer_statement" => await ExecuteGetCustomerStatementAsync(arguments, cancellationToken),
                "search_invoices" => await ExecuteSearchInvoicesAsync(arguments, cancellationToken),
                "get_invoice_details" => await ExecuteGetInvoiceDetailsAsync(arguments, cancellationToken),
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

        foreach (var pr in prices)
        {
            var formattedPrice = string.IsNullOrWhiteSpace(pr.Value) ? "نامشخص" : $"{pr.Value} {pr.Unit}";
            var changeStr = string.IsNullOrWhiteSpace(pr.Change) ? "-" : pr.Change;
            var dateStr = pr.LastUpdate.HasValue ? pr.LastUpdate.Value.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture) : "-";
            sb.AppendLine($"| {pr.Title} | {formattedPrice} | {changeStr} | {dateStr} |");
        }

        return McpContentResult.Text(sb.ToString());
    }

    private McpContentResult ExecuteCalculateGoldProductPrice(JsonElement args)
    {
        var weight = args.GetProperty("weight").GetDecimal();
        var fineness = args.GetProperty("fineness").GetDecimal();
        var gramPrice = args.GetProperty("gramPrice").GetDecimal();
        var profitPercent = args.GetProperty("profitPercent").GetDecimal();
        var taxPercent = args.GetProperty("taxPercent").GetDecimal();

        var wageTypeStr = args.TryGetProperty("wageType", out var wt) ? wt.GetString() : "Percent";
        var wageType = Enum.TryParse<WageType>(wageTypeStr, true, out var parsedWt) ? parsedWt : WageType.Percent;
        var wageAmount = args.TryGetProperty("wageAmount", out var wa) ? wa.GetDecimal() : 0m;
        var stoneAmount = args.TryGetProperty("stoneAmount", out var sa) ? sa.GetDecimal() : 0m;

        var rawPrice = CalculatorHelper.Product.CalculateRawPrice(weight, gramPrice, fineness, 1, ProductType.Gold);
        var wage = CalculatorHelper.Product.CalculateWage(rawPrice, weight, wageAmount, wageType, 1);
        var profit = CalculatorHelper.Product.CalculateProfit(rawPrice, wage, ProductType.Gold, profitPercent);
        var tax = CalculatorHelper.Product.CalculateTax(wage, profit, taxPercent, ProductType.Gold, stoneAmount);
        var finalPrice = CalculatorHelper.Product.CalculateFinalPrice(rawPrice, wage, profit, tax, stoneAmount, ProductType.Gold);

        var sb = new StringBuilder();
        sb.AppendLine("### 💍 ریز محاسبات قیمت طلا/جواهر:");
        sb.AppendLine($"- **وزن:** {weight:N3} گرم (عیار: {fineness})");
        sb.AppendLine($"- **نرخ پایه هر گرم:** {gramPrice:N0}");
        sb.AppendLine($"- **قیمت طلای خام:** {rawPrice:N0}");
        sb.AppendLine($"- **اجرت ساخت ({wageType}):** {wage:N0}");
        sb.AppendLine($"- **سود فروشنده ({profitPercent}%):** {profit:N0}");
        sb.AppendLine($"- **مالیات بر ارزش افزوده ({taxPercent}%):** {tax:N0}");
        if (stoneAmount > 0)
            sb.AppendLine($"- **ارزش سنگ/نگین:** {stoneAmount:N0}");
        sb.AppendLine($"\n**💳 مبلغ نهایی قابل پرداخت:** {finalPrice:N0}");

        return McpContentResult.Text(sb.ToString());
    }

    private McpContentResult ExecuteCalculateScrapGoldValuation(JsonElement args)
    {
        var weight = args.GetProperty("weight").GetDecimal();
        var fineness = args.GetProperty("fineness").GetDecimal();
        var deduction = args.GetProperty("deductionFrom750").GetDecimal();
        var gramPrice750 = args.GetProperty("gramPrice750").GetDecimal();

        var totalPayable = CalculatorHelper.UsedProduct.Calculate(weight, fineness, deduction, gramPrice750);
        var equivalentWeight750 = weight * (fineness / 750m);
        var effectiveWeight = equivalentWeight750 * ((750m - deduction) / 750m);

        var sb = new StringBuilder();
        sb.AppendLine("### 🪙 ارزیابی و خرید طلای کهنه/متفرقه:");
        sb.AppendLine($"- **وزن ناخالص:** {weight:N3} گرم");
        sb.AppendLine($"- **عیار واقعی:** {fineness}");
        sb.AppendLine($"- **کسری عیار توافقی از ۷۵۰:** {deduction}");
        sb.AppendLine($"- **وزن معادل ۷۵۰:** {equivalentWeight750:N3} گرم");
        sb.AppendLine($"- **وزن موثر پس از کسر عیار:** {effectiveWeight:N3} گرم");
        sb.AppendLine($"- **نرخ طلای ۷۵۰:** {gramPrice750:N0}");
        sb.AppendLine($"\n**💰 مبلغ کل قابل پرداخت به مشتری:** {totalPayable:N0}");

        return McpContentResult.Text(sb.ToString());
    }

    private McpContentResult ExecuteCalculateMoltenGold(JsonElement args)
    {
        var hasBudget = args.TryGetProperty("targetBudget", out var tb) && tb.GetDecimal() > 0;
        var gramPrice = args.TryGetProperty("gramPrice", out var gp) ? gp.GetDecimal() : 0m;
        var fineness = args.TryGetProperty("fineness", out var fn) ? fn.GetDecimal() : 750m;

        if (hasBudget && gramPrice > 0)
        {
            var budget = tb.GetDecimal();
            var weight = CalculatorHelper.MoltenGold.CalculateWeight(budget, fineness, gramPrice, 0);
            return McpContentResult.Text($"با بودجه **{budget:N0}** و عیار **{fineness}** در نرخ **{gramPrice:N0}**، مقدار **{weight:N3} گرم** طلای آب‌شده قابل خریداری است.");
        }

        var weightInput = args.TryGetProperty("weight", out var w) ? w.GetDecimal() : 0m;
        if (weightInput > 0 && gramPrice > 0)
        {
            var value = CalculatorHelper.MoltenGold.Calculate(weightInput, fineness, gramPrice, 1);
            var eq750 = weightInput * (fineness / 750m);
            return McpContentResult.Text($"طلای آب‌شده به وزن **{weightInput:N3} گرم** با عیار **{fineness}** (معادل **{eq750:N3} گرم** طلای ۱۸ عیار) به ارزش **{value:N0}** می‌باشد.");
        }

        return McpContentResult.Text("لطفاً وزن یا بودجه مورد نظر را به همراه نرخ روز وارد کنید.", isError: true);
    }

    private async Task<McpContentResult> ExecuteSearchInventoryStockAsync(JsonElement args, CancellationToken ct)
    {
        var query = args.TryGetProperty("query", out var q) ? q.GetString() : null;
        var pageSize = args.TryGetProperty("pageSize", out var ps) ? ps.GetInt32() : 15;

        var requestFilter = new RequestFilter
        {
            Search = query,
            Take = pageSize,
            Skip = 0
        };

        var inventoryFilter = new InventoryFilter(null, null, null, null, null);
        var pagedResult = await inventoryStockService.GetListAsync(requestFilter, inventoryFilter, ct);

        if (pagedResult.Data.Count == 0)
            return McpContentResult.Text("هیچ کالایی با مشخصات جستجو در انبار یافت نشد.");

        var sb = new StringBuilder();
        sb.AppendLine($"### 📦 نتایج جستجوی انبار ({pagedResult.Total} مورد):");
        sb.AppendLine("| بارکد/شناسه | عنوان کالا | نوع کالا | عیار | وزن موجود (گرم) |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");

        foreach (var item in pagedResult.Data)
        {
            var title = item.Product?.Name ?? item.Coin?.Coin?.Title ?? item.Currency?.Title ?? "کالا";
            var barcode = item.Product?.Barcode ?? item.Coin?.Barcode ?? "-";
            var type = item.Product?.ProductType.ToString() ?? (item.Coin != null ? "سکه" : item.Currency != null ? "ارز" : "-");
            var fineness = item.Product?.Fineness.ToString() ?? item.Coin?.Fineness.ToString() ?? "-";
            var weight = item.Product != null ? item.Product.Weight.ToString("N3") : item.Coin != null ? item.Coin.Weight.ToString("N3") : item.CurrentAmount.ToString("N2");

            sb.AppendLine($"| {barcode} | {title} | {type} | {fineness} | {weight} |");
        }

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteGetCustomerBalanceAsync(JsonElement args, CancellationToken ct)
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
            return McpContentResult.Text("مشتری مورد نظر یافت نشد. لطفاً نام یا شناسه صحیح را وارد کنید.", isError: true);

        var customer = await customerService.GetAsync(customerId.Value, ct);
        var balances = await transactionService.GetCustomerRemainingListAsync(customerId.Value, null, null, null, ct);

        var sb = new StringBuilder();
        sb.AppendLine($"### 👤 مانده حساب مشتری: **{customer.FullName}** ({customer.CustomerType})");
        sb.AppendLine($"- شماره تماس: {customer.PhoneNumber ?? "ثبت نشده"}");
        sb.AppendLine($"- کد ملی: {customer.NationalId ?? "ثبت نشده"}");
        sb.AppendLine("\n**وضعیت مانده حساب به تفکیک واحدها:**");

        if (balances.Count == 0)
        {
            sb.AppendLine("- حساب کاملاً تسویه است (مانده صفر).");
        }
        else
        {
            foreach (var b in balances)
            {
                var status = b.Amount > 0 ? "🔴 بدهکار به فروشگاه" : b.Amount < 0 ? "🟢 بستانکار از فروشگاه" : "⚪ تسویه";
                var unitName = b.PriceUnit?.Title ?? "واحد نامشخص";
                sb.AppendLine($"- **{unitName}:** {Math.Abs(b.Amount):N3} ({status})");
            }
        }

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteSearchCustomersAsync(JsonElement args, CancellationToken ct)
    {
        var query = args.GetProperty("query").GetString();
        var matches = await customerService.GetByNameAsync(query, null, ct);

        if (matches.Count == 0)
            return McpContentResult.Text($"هیچ مشتری با عبارت «{query}» یافت نشد.");

        var sb = new StringBuilder();
        sb.AppendLine($"### 👥 لیست مشتریان منطبق ({matches.Count} مورد):");
        sb.AppendLine("| نام و نام خانوادگی | نوع مشتری | شماره همراه | کد ملی |");
        sb.AppendLine("| :--- | :--- | :--- | :--- |");

        foreach (var c in matches)
        {
            sb.AppendLine($"| {c.FullName} | {c.CustomerType} | {c.PhoneNumber ?? "-"} | {c.NationalId ?? "-"} |");
        }

        return McpContentResult.Text(sb.ToString());
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

        var request = new CustomerTransactionRpRequest(customerId.Value, null, null, null, null);
        var transactions = await reportingService.GetCustomerTransactionsAsync(request, ct);

        if (transactions.Count == 0)
            return McpContentResult.Text("هیچ تراکنشی برای این مشتری در بازه زمانی مورد نظر ثبت نشده است.");

        var sb = new StringBuilder();
        sb.AppendLine($"### 📜 ریز تراکنش‌های دفتر کل مشتری ({transactions.Count} تراکنش):");
        sb.AppendLine("| تاریخ | شرح تراکنش | بدهکار | بستانکار | مانده | واحد |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- |");

        foreach (var t in transactions.Take(25))
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
        var filter = new RequestFilter { Take = 15, Skip = 0 };
        InvoiceType? targetInvoiceType = null;

        if (args.TryGetProperty("invoiceType", out var itProp) && Enum.TryParse<InvoiceType>(itProp.GetString(), true, out var it))
            targetInvoiceType = it;

        var invoiceFilter = new InvoiceFilter(null, targetInvoiceType, null, null, null);
        var list = await invoiceService.GetListAsync(filter, invoiceFilter, null, ct);

        if (list.Data.Count == 0)
            return McpContentResult.Text("هیچ فاکتوری با فیلترهای مشخص‌شده یافت نشد.");

        var sb = new StringBuilder();
        sb.AppendLine($"### 🧾 لیست فاکتورها ({list.Total} مورد):");
        sb.AppendLine("| شماره فاکتور | نوع | مشتری | تاریخ | مبلغ کل | واحد | وضعیت پرداخت |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- |");

        foreach (var inv in list.Data)
        {
            var dateStr = inv.InvoiceDate.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            sb.AppendLine($"| {inv.InvoiceNumber} | {inv.InvoiceType} | {inv.CustomerFullName} | {dateStr} | {inv.TotalAmount:N0} | {inv.PriceUnit} | {inv.PaymentStatus} |");
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
            return McpContentResult.Text("فاکتور مورد نظر یافت نشد.", isError: true);

        var unitTitle = invoice.PriceUnit?.Title ?? "";
        var unpaidUnitTitle = invoice.UnpaidPriceUnit?.Title ?? unitTitle;

        var sb = new StringBuilder();
        sb.AppendLine($"### 📄 جزئیات فاکتور شماره {invoice.InvoiceNumber} ({invoice.InvoiceType})");
        sb.AppendLine($"- **مشتری:** {invoice.Customer?.FullName ?? "عمومی"}");
        sb.AppendLine($"- **تاریخ ثبت:** {invoice.InvoiceDate:yyyy/MM/dd}");
        sb.AppendLine($"- **مبلغ کل اقلام:** {invoice.TotalAmount:N0} {unitTitle}");
        sb.AppendLine($"- **تخفیف:** {invoice.TotalDiscountAmount:N0} {unitTitle}");
        sb.AppendLine($"- **مبلغ نهایی با کسورات و اضافات:** {invoice.TotalAmountWithDiscountsAndExtraCosts:N0} {unitTitle}");
        sb.AppendLine($"- **مانده تسویه‌نشده:** {invoice.TotalUnpaidAmount:N0} {unpaidUnitTitle}");

        if (invoice.InvoiceProductItems?.Count > 0)
        {
            sb.AppendLine("\n**اقلام فاکتور:**");
            sb.AppendLine($"| ردیف | شرح کالا | وزن (گرم) | عیار | اجرت ({unitTitle}) | سود ({unitTitle}) | مبلغ نهایی ({unitTitle}) |");
            sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- |");
            int idx = 1;
            foreach (var itm in invoice.InvoiceProductItems)
            {
                sb.AppendLine($"| {idx++} | {itm.Product?.Name ?? "کالای طلا"} | {itm.TotalWeight:N3} | {itm.Product?.Fineness} | {itm.ItemWageAmount:N0} | {itm.ItemProfitAmount:N0} | {itm.ItemFinalAmount:N0} |");
            }
        }

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteGetTrialBalanceReportAsync(JsonElement args, CancellationToken ct)
    {
        var request = new LedgerAccountTrialBalanceRpRequest(null, null, null);
        var report = await reportingService.GetLedgerAccountTrialBalanceAsync(request, ct);

        if (report.Count == 0)
            return McpContentResult.Text("داده‌ای برای تراز آزمایشی یافت نشد.");

        var sb = new StringBuilder();
        sb.AppendLine($"### ⚖️ تراز آزمایشی حسابداری ({report.Count} سرفصل):");
        sb.AppendLine("| عنوان حساب | نوع حساب | بدهکار | بستانکار | واحد مبنا |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");

        foreach (var r in report.Take(30))
        {
            sb.AppendLine($"| {r.LedgerAccountTitle} | {r.LedgerAccountType} | {r.DebitAmountBase:N0} | {r.CreditAmountBase:N0} | {r.BasePriceUnitTitle} |");
        }

        return McpContentResult.Text(sb.ToString());
    }

    private async Task<McpContentResult> ExecuteGetUsedGoldHiddenProfitAsync(JsonElement args, CancellationToken ct)
    {
        var request = new UsedGoldHiddenProfitRpRequest(null, null, null, null);
        var report = await reportingService.GetUsedGoldHiddenProfitAsync(request, ct);

        if (report.Count == 0)
            return McpContentResult.Text("هیچ رکورد سود پنهانی در بازه زمانی تعیین‌شده یافت نشد.");

        var sb = new StringBuilder();
        sb.AppendLine("### 💰 گزارش سود پنهان ری‌گیری و آب کردن طلای کهنه:");
        sb.AppendLine("| شماره فاکتور | تاریخ | نام مشتری | وزن (گرم) | عیار اسمی | مبلغ پرداختی | ارزش واقعی | سود پنهان | واحد |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |");

        foreach (var r in report)
        {
            sb.AppendLine($"| {r.InvoiceNumber} | {r.InvoiceDate} | {r.CustomerName} | {r.Weight:N3} | {r.Fineness} | {r.PaidAmount:N0} | {r.RealValue:N0} | {r.HiddenProfit:N0} | {r.PriceUnitTitle} |");
        }

        return McpContentResult.Text(sb.ToString());
    }
}
