using GoldEx.Server.Mcp;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text.Json;

namespace GoldEx.Tests.Server.Application.Services;

[TestFixture]
public class McpFinancialReportTester
{
    private static JsonElement Args(object payload)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(payload));
        return document.RootElement.Clone();
    }

    private static GoldExMcpEngine Engine(IReportingService? reportingService = null,
        IInvoiceService? invoiceService = null) => new(
        Mock.Of<IPriceService>(), Mock.Of<IInventoryStockService>(),
        Mock.Of<ICustomerService>(), Mock.Of<ITransactionService>(),
        reportingService ?? Mock.Of<IReportingService>(),
        invoiceService ?? Mock.Of<IInvoiceService>(), Mock.Of<IPriceUnitService>(),
        Mock.Of<IFinancialAccountService>(), Mock.Of<IHttpContextAccessor>());

    private static CustomerTransactionRpResponse Row(DateTime date, string desc,
        TransactionType type, decimal amount, decimal running) =>
        new(date, desc, type, LedgerAccountRole.Receivable,
            amount, running, "گرم", null, 0m, null, null, null);

    [Test]
    public void DecimalFormatting_Exact()
    {
        Assert.Multiple(() =>
        {
            Assert.That(McpFinancialReportFormatting.Exact(12.6798m), Is.EqualTo("12.6798"));
            Assert.That(McpFinancialReportFormatting.Exact(1.3578m), Is.EqualTo("1.3578"));
            Assert.That(McpFinancialReportFormatting.Exact(10001.123456m), Is.EqualTo("10,001.123456"));
            Assert.That(McpFinancialReportFormatting.Exact(0m), Is.EqualTo("0"));
        });
    }

    [Test]
    public async Task Statement_IncludesWholeEndDate_CarriesOpening()
    {
        var id = Guid.NewGuid();
        var history = new[]
        {
            Row(new DateTime(2026,11,6,12,0,0), "previous", TransactionType.Credit, 0.4m,-0.4m),
            Row(new DateTime(2026,11,8,16,25,0), "invoice-1786", TransactionType.Debit, 12.6798m,12.2798m)
        };
        var report = new Mock<IReportingService>();
        report.Setup(x => x.GetCustomerTransactionsAsync(
                It.IsAny<CustomerTransactionRpRequest>(), It.IsAny<CancellationToken>()))
            .Returns((CustomerTransactionRpRequest request, CancellationToken _) =>
                Task.FromResult(history.Where(t => !request.ToDate.HasValue ||
                    t.PostingDate <= request.ToDate.Value).ToList()));
        var result = await Engine(report.Object).CallToolAsync("get_customer_statement",
            Args(new { customerId = id, fromDate = "1405/08/17", toDate = "1405/08/17" }));
        Assert.That(result.IsError, Is.False, result.Content[0].Text);
        Assert.Multiple(() =>
        {
            Assert.That(result.Content[0].Text, Does.Contain("1 تراکنش در بازه"));
            Assert.That(result.Content[0].Text, Does.Contain("افتتاحیه **-0.4**"));
            Assert.That(result.Content[0].Text, Does.Contain("اختتامیه **12.2798**"));
            Assert.That(result.Content[0].Text, Does.Contain("invoice-1786"));
            Assert.That(result.Content[0].Text, Does.Contain("12.6798"));
            Assert.That(result.Content[0].Text, Does.Not.Contain("previous |"));
        });
        report.Verify(x => x.GetCustomerTransactionsAsync(
            It.Is<CustomerTransactionRpRequest>(r => !r.FromDate.HasValue &&
                r.ToDate.HasValue && r.ToDate.Value.Date == new DateTime(2026,11,8) &&
                r.ToDate.Value.Hour == 23 && r.ToDate.Value.Minute == 59),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Statement_DoesNotSilentlyDropEntriesAfter50()
    {
        var id = Guid.NewGuid();
        var rows = Enumerable.Range(0,91)
            .Select(i => Row(new DateTime(2026,11,8,10,0,0).AddMinutes(i),
                $"entry-{i:D3}", TransactionType.Debit, 0.01m, (i+1)/100m)).ToList();
        var report = new Mock<IReportingService>();
        report.Setup(x => x.GetCustomerTransactionsAsync(
                It.IsAny<CustomerTransactionRpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows);
        var engine = Engine(report.Object);
        var full = await engine.CallToolAsync("get_customer_statement", Args(new { customerId=id }));
        var page1 = await engine.CallToolAsync("get_customer_statement",
            Args(new { customerId=id, pageSize=50, page=1 }));
        var page2 = await engine.CallToolAsync("get_customer_statement",
            Args(new { customerId=id, pageSize=50, page=2 }));
        Assert.Multiple(() =>
        {
            Assert.That(full.IsError, Is.False);
            Assert.That(full.Content[0].Text, Does.Contain("91 تراکنش در بازه"));
            Assert.That(full.Content[0].Text, Does.Contain("entry-090"));
            Assert.That(full.Content[0].Text, Does.Contain("اختتامیه **0.91**"));
            Assert.That(page1.Content[0].Text, Does.Contain("صفحه:** 1 از 2"));
            Assert.That(page1.Content[0].Text, Does.Contain("این گزارش کامل نیست"));
            Assert.That(page1.Content[0].Text, Does.Not.Contain("entry-090"));
            Assert.That(page2.Content[0].Text, Does.Contain("entry-090"));
        });
    }

    [Test]
    public async Task Statement_EmptyWindow_StillShowsHistoricalBalance()
    {
        var id=Guid.NewGuid();
        var report=new Mock<IReportingService>();
        report.Setup(x => x.GetCustomerTransactionsAsync(
                It.IsAny<CustomerTransactionRpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CustomerTransactionRpResponse> {
                Row(new DateTime(2026,10,1), "old",TransactionType.Debit,3.12m,3.12m)});
        var r=await Engine(report.Object).CallToolAsync("get_customer_statement",
            Args(new { customerId=id, fromDate="1405/08/17",toDate="1405/08/17" }));
        Assert.That(r.IsError, Is.False);
        Assert.That(r.Content[0].Text, Does.Contain("افتتاحیه **3.12**"));
        Assert.That(r.Content[0].Text, Does.Contain("اختتامیه **3.12**"));
    }

    [Test]
    public async Task InvoiceDetails_KeepsFractionalAmounts()
    {
        var unit=new GetPriceUnitTitleResponse(Guid.NewGuid(),"گرم",false,true,true);
        var item=new GetInvoiceProductItemResponse(
            Id:Guid.NewGuid(),GramPrice:1m,ProfitPercent:0m,TaxPercent:0m,
            WageExchangeRate:null,CostPrice:null,CostPriceExchangeRate:null,
            CostPriceUnitId:null,CostPriceUnitTitle:null,
            SaleWage:9.5m,SaleWageType:WageType.Percent,SaleWagePriceUnitId:null,
            SaleWagePriceUnitTitle:null,PurchaseWage:null,PurchaseWageType:null,
            PurchaseWagePriceUnitId:null,PurchaseWagePriceUnitTitle:null,
            PurchaseWagePriceUnitExchangeRate:null,StonePriceUnitExchangeRate:null,
            IsInstantProduct:false,Quantity:1,TotalWeight:1.24m,
            ItemRawAmount:1.24m,ItemWageAmount:0.1178m,
            ItemProfitAmount:0m,ItemTaxAmount:0m,ItemFinalAmount:1.3578m,
            TotalAmount:1.3578m,TotalStoneAmount:null,Product:null!);
        var invoice=new GetInvoiceResponse(
            Id:Guid.NewGuid(),InvoiceNumber:1786,InvoiceDate:new DateTime(2026,11,8),
            DueDate:null,InvoiceType:InvoiceType.Sell,TradeScale:TradeScale.Wholesale,
            TotalAmount:12.6798m,TotalPaidAmount:0m,TotalDiscountAmount:0m,
            TotalExtraCostAmount:0m,TotalUnpaidAmount:12.6798m,
            TotalUsedProductsAmount:0m,TotalAmountWithDiscountsAndExtraCosts:12.6798m,
            UnpaidAmountExchangeRate:null,ExchangeRate:null,
            UnpaidPriceUnit:unit,PriceUnit:unit,Customer:null!,
            InvoiceProductItems:[item],InvoiceCoinItems:[],InvoiceCurrencyItems:[],
            InvoiceDiscounts:[],InvoicePayments:[],InvoiceExtraCosts:[],InvoiceUsedProducts:[]);
        var svc=new Mock<IInvoiceService>();
        svc.Setup(x=>x.GetAsync(1786,InvoiceType.Sell,It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);
        var r=await Engine(invoiceService:svc.Object).CallToolAsync(
            "get_invoice_details",Args(new { invoiceNumber=1786,invoiceType="Sell" }));
        Assert.That(r.IsError,Is.False,r.Content[0].Text);
        Assert.That(r.Content[0].Text,Does.Contain("12.6798 گرم"));
        Assert.That(r.Content[0].Text,Does.Contain("1.3578 گرم"));
        Assert.That(r.Content[0].Text,Does.Contain("0.1178"));
        Assert.That(r.Content[0].Text,Does.Not.Contain("**13 گرم**"));
    }
}
