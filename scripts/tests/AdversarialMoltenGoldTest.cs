using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

class Program
{
    static int passed = 0;
    static int failed = 0;
    static List<string> findings = new();

    static void Record(string testName, bool condition, string detail)
    {
        if (condition)
        {
            passed++;
            Console.WriteLine($"[PASS] {testName} : {detail}");
        }
        else
        {
            failed++;
            string msg = $"[FAIL] {testName} : {detail}";
            findings.Add(msg);
            Console.WriteLine(msg);
        }
    }

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=================================================");
        Console.WriteLine("ADVERSARIAL STRESS SUITE: Molten Gold Edge Cases");
        Console.WriteLine("=================================================");

        TestDivisionByZeroAndClamping();
        TestFinenessAndPrecision();
        TestShamsiDateConversions();
        TestFilteredRowsSearchAndNulls();
        TestTradeDetailsNullAndBoundaries();

        Console.WriteLine("\n=================================================");
        Console.WriteLine($"TOTAL ASSERTIONS: {passed + failed}");
        Console.WriteLine($"PASSED: {passed}");
        Console.WriteLine($"FAILED / SURFACED VULNERABILITIES: {failed}");
        Console.WriteLine("=================================================");
        foreach (var f in findings)
        {
            Console.WriteLine("  * " + f);
        }
    }

    // -----------------------------------------------------------------------------------------
    // 1. Division by Zero & Math.Clamp
    // -----------------------------------------------------------------------------------------
    static void TestDivisionByZeroAndClamping()
    {
        Console.WriteLine("\n--- 1. Division by Zero & Math.Clamp ---");

        // Code in MoltenGoldTradeDetails.razor line 83-91:
        // @if (Trade.Total > 0)
        // {
        //     var paidPercent = Math.Clamp((int)((Trade.Total - Trade.RemainingAmount) / Trade.Total * 100m), 0, 100);
        // }

        // Case 1.1: Trade.Total == 0
        decimal total1 = 0m;
        decimal remaining1 = 0m;
        bool guarded1 = total1 > 0;
        Record("Trade.Total == 0 is guarded by total > 0", !guarded1, "Total == 0 will not evaluate division");

        // Case 1.2: Trade.Total < 0
        decimal total2 = -100m;
        bool guarded2 = total2 > 0;
        Record("Trade.Total < 0 is guarded by total > 0", !guarded2, "Negative total will not evaluate division");

        // Case 1.3: Trade.Weight == 0
        decimal weight1 = 0m;
        bool guardedWeight = weight1 > 0;
        Record("Trade.Weight == 0 is guarded by weight > 0", !guardedWeight, "Weight == 0 will not evaluate division");

        // Case 1.4: Normal paidPercent (50%)
        decimal totalNormal = 100_000m;
        decimal remainingNormal = 50_000m;
        int pctNormal = Math.Clamp((int)((totalNormal - remainingNormal) / totalNormal * 100m), 0, 100);
        Record("Normal PaidPercent (50%)", pctNormal == 50, $"Computed: {pctNormal}%");

        // Case 1.5: Overpayment (RemainingAmount < 0)
        // E.g. Total = 100,000, RemainingAmount = -20,000 (customer overpaid)
        decimal remainingOverpaid = -20_000m;
        int pctOverpaid = Math.Clamp((int)((totalNormal - remainingOverpaid) / totalNormal * 100m), 0, 100);
        Record("Overpayment clamped to 100%", pctOverpaid == 100, $"Computed: {pctOverpaid}%");

        // Case 1.6: Debt expansion (RemainingAmount > Total)
        // E.g. penalties or negative adjustments where RemainingAmount = 150,000
        decimal remainingExpanded = 150_000m;
        int pctExpanded = Math.Clamp((int)((totalNormal - remainingExpanded) / totalNormal * 100m), 0, 100);
        Record("Debt expansion clamped to 0%", pctExpanded == 0, $"Computed: {pctExpanded}%");

        // Case 1.7: Extreme boundary - Small Total & Large Remaining (Int32 overflow before clamp)
        try
        {
            decimal smallTotal = 1m;
            decimal largeRemaining = 100_000_000_000m; // 100 billion Toman open balance
            // Evaluates: (int)((smallTotal - largeRemaining) / smallTotal * 100m)
            decimal ratio = (smallTotal - largeRemaining) / smallTotal * 100m; // -10,000,000,000,000m
            int casted = (int)ratio; // Throws OverflowException because ratio < int.MinValue (-2,147,483,648)
            int clamped = Math.Clamp(casted, 0, 100);
            Record("Small Total + Large Remaining survived cast", true, $"Result: {clamped}");
        }
        catch (OverflowException ex)
        {
            Record("Int32 Cast Overflow before Math.Clamp", false,
                $"Casting decimal ratio to (int) before Math.Clamp throws OverflowException when ratio exceeds Int32 bounds: {ex.Message}");
        }
    }

    // -----------------------------------------------------------------------------------------
    // 2. Fineness & Decimal Precision
    // -----------------------------------------------------------------------------------------
    static void TestFinenessAndPrecision()
    {
        Console.WriteLine("\n--- 2. Fineness & Decimal Precision ---");

        // Code in MoltenGoldTradeDetails.razor line 68:
        // (Trade.Weight * Trade.Fineness / 750m).ToString("0.###")
        // and MoltenGoldDesk.razor line 125:
        // WeightText(_weight * _fineness / 750m)

        decimal weight = 125.456m;
        decimal fineness = 750m;
        decimal eq750 = weight * fineness / 750m;
        Record("750 fineness equivalent equality", eq750 == weight, $"eq750={eq750}, weight={weight}");

        // Non-standard fineness 740, 705
        decimal eq740 = 100m * 740m / 750m;
        string str740 = eq740.ToString("0.###");
        Record("740 fineness formatting (0.###)", str740 == "98.667", $"str740={str740}");

        // Fineness 0
        decimal eqZeroFineness = 100m * 0m / 750m;
        Record("Fineness == 0 does not divide by zero", eqZeroFineness == 0m, $"Result: {eqZeroFineness}");

        // Maximum domain weight (1,000,000 grams) with max fineness (999.999)
        decimal maxDomainWeight = 1_000_000m;
        decimal maxFineness = 999.999m;
        decimal eqMax = maxDomainWeight * maxFineness / 750m;
        Record("Max domain weight (1 ton) & fineness precision", eqMax == 1333332m, $"eqMax={eqMax}");

        // Decimal.MaxValue boundary
        try
        {
            decimal max = decimal.MaxValue;
            decimal multiplier = 2m;
            decimal overflowVal = max * multiplier;
            Record("Decimal.MaxValue boundary", false, "Did not throw");
        }
        catch (OverflowException)
        {
            Record("Decimal.MaxValue throws OverflowException as expected", true, "Decimal overflow is caught");
        }
    }

    // -----------------------------------------------------------------------------------------
    // 3. Shamsi Date Conversion Edge Cases
    // -----------------------------------------------------------------------------------------
    static void TestShamsiDateConversions()
    {
        Console.WriteLine("\n--- 3. Shamsi Date Conversions ---");

        // Code in MoltenGoldTradeDetails.razor line 266-276:
        // var cal = new PersianCalendar();
        // return $"{cal.GetYear(date)}/{cal.GetMonth(date):00}/{cal.GetDayOfMonth(date):00}";

        string FormatShamsi(DateTime date)
        {
            var cal = new PersianCalendar();
            return $"{cal.GetYear(date)}/{cal.GetMonth(date):00}/{cal.GetDayOfMonth(date):00}";
        }

        string FormatShamsiDateTime(DateTime date)
        {
            var cal = new PersianCalendar();
            return $"{cal.GetYear(date)}/{cal.GetMonth(date):00}/{cal.GetDayOfMonth(date):00} · {date:HH:mm}";
        }

        // 3.1: Nowruz (Farvardin 1) - 2024-03-20
        var nowruz1403 = FormatShamsi(new DateTime(2024, 3, 20));
        Record("Nowruz 1403 (2024-03-20) -> 1403/01/01", nowruz1403 == "1403/01/01", $"Result: {nowruz1403}");

        // 3.2: Persian Leap Year Esfand 30 (1403 was leap year, 2025-03-20)
        var leapEsfand30 = FormatShamsi(new DateTime(2025, 3, 20));
        Record("Leap year 1403 Esfand 30 (2025-03-20) -> 1403/12/30", leapEsfand30 == "1403/12/30", $"Result: {leapEsfand30}");

        // 3.3: Non-Leap Year Esfand 29 (1404 is non-leap year, 2026-03-20)
        var nonLeapEsfand29 = FormatShamsi(new DateTime(2026, 3, 20));
        Record("Non-leap year 1404 Esfand 29 (2026-03-20) -> 1404/12/29", nonLeapEsfand29 == "1404/12/29", $"Result: {nonLeapEsfand29}");

        // 3.4: Day after non-leap Esfand 29 is Farvardin 1 of next year (2026-03-21)
        var farvardin1405 = FormatShamsi(new DateTime(2026, 3, 21));
        Record("Next year Farvardin 1 (2026-03-21) -> 1405/01/01", farvardin1405 == "1405/01/01", $"Result: {farvardin1405}");

        // 3.5: Time formatting edge cases (00:00 and 23:59)
        var timeMidnight = FormatShamsiDateTime(new DateTime(2026, 11, 7, 0, 0, 0));
        Record("Midnight time 00:00 formatting", timeMidnight.EndsWith("00:00"), $"Result: {timeMidnight}");

        var timeEnd = FormatShamsiDateTime(new DateTime(2026, 11, 7, 23, 59, 0));
        Record("End of day 23:59 formatting", timeEnd.EndsWith("23:59"), $"Result: {timeEnd}");

        // 3.6: DateTime.MinValue (default DateTime)
        // If an uninitialized Trade.Date or _rateUpdatedAt (DateTime.MinValue) is passed:
        try
        {
            FormatShamsi(DateTime.MinValue);
            Record("DateTime.MinValue in PersianCalendar", false, "Did not throw exception");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Record("DateTime.MinValue throws ArgumentOutOfRangeException in PersianCalendar", false,
                $"VULNERABILITY: PersianCalendar fails on DateTime.MinValue (default(DateTime)): {ex.Message}");
        }

        // 3.7: DateTime.MaxValue
        try
        {
            var maxStr = FormatShamsi(DateTime.MaxValue);
            Record("DateTime.MaxValue supported in PersianCalendar", maxStr.StartsWith("9378"), $"Result: {maxStr}");
        }
        catch (Exception ex)
        {
            Record("DateTime.MaxValue error", false, ex.Message);
        }
    }

    // -----------------------------------------------------------------------------------------
    // 4. FilteredRows Search & Null Handling in MoltenGoldDesk.razor.cs
    // -----------------------------------------------------------------------------------------
    class TradeDto
    {
        public string? CustomerName { get; set; }
        public decimal Weight { get; set; }
        public List<EventDto>? Events { get; set; }
    }

    class EventDto
    {
        public string? Reference { get; set; }
    }

    static void TestFilteredRowsSearchAndNulls()
    {
        Console.WriteLine("\n--- 4. FilteredRows Search & Null Handling ---");

        // Code in MoltenGoldDesk.razor.cs line 48-61:
        // private IReadOnlyList<MoltenGoldTradeResponse> FilteredRows
        // {
        //     get
        //     {
        //         if (_rows is null) return [];
        //         if (string.IsNullOrWhiteSpace(_searchText)) return _rows;
        //         var query = _searchText.Trim();
        //         return _rows.Where(r =>
        //             (r.CustomerName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
        //             r.Events.Any(e => e.Reference?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
        //             r.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture).Contains(query, StringComparison.Ordinal)
        //         ).ToList();
        //     }
        // }

        List<TradeDto>? ExecuteFilter(List<TradeDto>? rows, string? searchText)
        {
            if (rows is null) return [];
            if (string.IsNullOrWhiteSpace(searchText)) return rows;
            var query = searchText.Trim();
            return rows.Where(r =>
                (r.CustomerName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                r.Events.Any(e => e.Reference?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                r.Weight.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.Ordinal)
            ).ToList();
        }

        // Test 4.1: rows is null
        var nullRowsResult = ExecuteFilter(null, "search");
        Record("FilteredRows returns empty list when rows is null", nullRowsResult != null && nullRowsResult.Count == 0, "Count: 0");

        // Test 4.2: searchText is null / empty / whitespace
        var sampleRows = new List<TradeDto>
        {
            new TradeDto { CustomerName = "علی محمدی", Weight = 125.45m, Events = new() { new() { Reference = "رسید بانک ملت" } } },
            new TradeDto { CustomerName = "زرگری پارس", Weight = 50.0m, Events = new() { new() { Reference = "حواله 123" } } }
        };

        Record("FilteredRows with null searchText returns all rows", ExecuteFilter(sampleRows, null)!.Count == 2, "Count: 2");
        Record("FilteredRows with empty searchText returns all rows", ExecuteFilter(sampleRows, "")!.Count == 2, "Count: 2");
        Record("FilteredRows with whitespace searchText returns all rows", ExecuteFilter(sampleRows, "   ")!.Count == 2, "Count: 2");

        // Test 4.3: Trim search text with surrounding whitespace
        var trimmedRes = ExecuteFilter(sampleRows, "  علی  ");
        Record("FilteredRows trims search query correctly", trimmedRes!.Count == 1 && trimmedRes[0].CustomerName == "علی محمدی", "Found 1 row");

        // Test 4.4: English digits in weight
        var weightRes = ExecuteFilter(sampleRows, "125.45");
        Record("FilteredRows finds weight with English digits", weightRes!.Count == 1, "Found 1 row");

        // Test 4.5: Persian digits in weight (۱۲۵.۴۵)
        var persianDigitsRes = ExecuteFilter(sampleRows, "۱۲۵.۴۵");
        Record("FilteredRows weight search with Persian digits ('۱۲۵.۴۵')", persianDigitsRes!.Count > 0,
            $"Found {persianDigitsRes!.Count} rows (InvariantCulture weight produces ASCII '125.45', Persian digits will NOT match!)");

        // Test 4.6: Arabic Yeh vs Persian Yeh ('علي' vs 'علی')
        // In Persian: \u06CC ('ی'). In Arabic: \u064A ('ي')
        string arabicAli = "علي"; // with Arabic Yeh
        var arabicYehRes = ExecuteFilter(sampleRows, arabicAli);
        Record("FilteredRows customer search with Arabic Yeh ('علي' vs 'علی')", arabicYehRes!.Count > 0,
            $"Found {arabicYehRes!.Count} rows (StringComparison.OrdinalIgnoreCase does not normalize Arabic Yeh to Persian Yeh)");

        // Test 4.7: Special characters (regex chars [*+?()\)
        try
        {
            var specialCharsRes = ExecuteFilter(sampleRows, "[*+?()\\");
            Record("FilteredRows handles special characters safely without regex crash", true, $"Count: {specialCharsRes!.Count}");
        }
        catch (Exception ex)
        {
            Record("FilteredRows special characters", false, ex.Message);
        }

        // Test 4.8: CustomerName is null
        var rowsWithNullCustomer = new List<TradeDto>
        {
            new TradeDto { CustomerName = null, Weight = 10m, Events = new() }
        };
        try
        {
            var resNullCust = ExecuteFilter(rowsWithNullCustomer, "search");
            Record("FilteredRows handles null CustomerName safely", resNullCust!.Count == 0, "No NRE thrown");
        }
        catch (Exception ex)
        {
            Record("FilteredRows with null CustomerName", false, ex.Message);
        }

        // Test 4.9: Events list is null!
        var rowsWithNullEvents = new List<TradeDto>
        {
            new TradeDto { CustomerName = "تست", Weight = 10m, Events = null }
        };
        try
        {
            ExecuteFilter(rowsWithNullEvents, "search");
            Record("FilteredRows with null Events list", true, "No exception thrown");
        }
        catch (ArgumentNullException ex)
        {
            Record("FilteredRows throws ArgumentNullException when Trade.Events is null", false,
                $"CRITICAL VULNERABILITY: r.Events.Any() throws ArgumentNullException (Value cannot be null, Parameter 'source') when r.Events is null: {ex.Message}");
        }
        catch (NullReferenceException ex)
        {
            Record("FilteredRows throws NullReferenceException when Trade.Events is null", false,
                $"CRITICAL VULNERABILITY: r.Events.Any() throws NullReferenceException when r.Events is null: {ex.Message}");
        }

        // Test 4.10: An item inside Events list is null!
        var rowsWithNullEventItem = new List<TradeDto>
        {
            new TradeDto { CustomerName = "تست", Weight = 10m, Events = new() { null! } }
        };
        try
        {
            ExecuteFilter(rowsWithNullEventItem, "search");
            Record("FilteredRows with null item inside Events list", true, "No exception thrown");
        }
        catch (NullReferenceException ex)
        {
            Record("FilteredRows throws NullReferenceException when item in Events is null", false,
                $"CRITICAL VULNERABILITY: e.Reference throws NullReferenceException when e is null: {ex.Message}");
        }

        // Test 4.11: An item inside Events has null Reference
        var rowsWithNullReference = new List<TradeDto>
        {
            new TradeDto { CustomerName = "تست", Weight = 10m, Events = new() { new EventDto { Reference = null } } }
        };
        try
        {
            var resNullRef = ExecuteFilter(rowsWithNullReference, "search");
            Record("FilteredRows handles null Event.Reference safely", resNullRef!.Count == 0, "Safe null propagation (e.Reference?.Contains)");
        }
        catch (Exception ex)
        {
            Record("FilteredRows with null Event.Reference", false, ex.Message);
        }
    }

    // -----------------------------------------------------------------------------------------
    // 5. MoltenGoldTradeDetails Null Handling & Balance States
    // -----------------------------------------------------------------------------------------
    static void TestTradeDetailsNullAndBoundaries()
    {
        Console.WriteLine("\n--- 5. MoltenGoldTradeDetails Null Handling & Balance States ---");

        // Test 5.1: Negative RemainingAmount display logic
        // In MoltenGoldTradeDetails.razor line 78-81:
        // @(Trade.RemainingAmount > 0 ? Trade.RemainingAmount.ToString("#,##0.##") : "تسویه شده")
        // If customer overpaid (RemainingAmount = -50,000), does it show "تسویه شده" or the overpaid amount?
        decimal overpaidAmount = -50_000m;
        string displayedAmount = overpaidAmount > 0 ? overpaidAmount.ToString("#,##0.##") : "تسویه شده";
        Record("Negative RemainingAmount (overpayment) displays as 'تسویه شده'", displayedAmount == "تسویه شده",
            $"Overpayment amount -50,000 is masked as '{displayedAmount}' because logic checks `> 0`");

        // Test 5.2: Negative RemainingWeight display logic
        // In MoltenGoldTradeDetails.razor line 100-103:
        // @(Trade.RemainingWeight > 0 ? Trade.RemainingWeight.ToString("0.###") : "تحویل کامل")
        decimal overDeliveredWeight = -5.25m;
        string displayedWeight = overDeliveredWeight > 0 ? overDeliveredWeight.ToString("0.###") : "تحویل کامل";
        Record("Negative RemainingWeight (over-delivery) displays as 'تحویل کامل'", displayedWeight == "تحویل کامل",
            $"Over-delivery weight -5.25 is masked as '{displayedWeight}' because logic checks `> 0`");

        // Test 5.3: IsFullySettled logic with negative balances
        // Line 258: bool IsFullySettled => Trade.RemainingAmount <= 0 && Trade.RemainingWeight <= 0;
        bool isFullySettledOverpaid = (-50_000m <= 0) && (-5.25m <= 0);
        Record("IsFullySettled is true when balances are negative (oversettled)", isFullySettledOverpaid,
            "Trade is treated as fully settled even when there is an overpayment/credit");

        // Test 5.4: Timeline null Reference and InvoiceNumber
        // Line 217: @if (entry.InvoiceNumber is long number)
        // Line 224: @if (entry.Kind != MoltenGoldTradeEventKind.GoldSettlement && !string.IsNullOrWhiteSpace(entry.Reference))
        long? nullInvoiceNum = null;
        bool invoiceMatch = nullInvoiceNum is long num;
        Record("entry.InvoiceNumber null pattern match", !invoiceMatch, "Pattern matching `is long num` safely handles null");

        string? nullRef = null;
        bool refMatch = !string.IsNullOrWhiteSpace(nullRef);
        Record("entry.Reference null check", !refMatch, "string.IsNullOrWhiteSpace safely handles null reference");

        // Test 5.5: Events is null in TradeDetails
        // Line 191: Trade.Events.Count
        // Line 204: foreach (var entry in Trade.Events.OrderByDescending(...))
        // Line 240: Trade.Events.All(...)
        List<EventDto>? nullEvents = null;
        try
        {
            int c = nullEvents!.Count;
            Record("Trade.Events is null in Details dialog", false, "Did not throw");
        }
        catch (NullReferenceException ex)
        {
            Record("Trade.Events is null throws NullReferenceException in Details dialog", false,
                $"CRITICAL VULNERABILITY: Trade.Events.Count / Trade.Events.OrderByDescending throws NRE if Events is null: {ex.Message}");
        }
    }
}
