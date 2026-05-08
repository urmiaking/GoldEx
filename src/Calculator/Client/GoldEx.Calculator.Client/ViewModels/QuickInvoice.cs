using GoldEx.Client.Components.Calculator.ViewModels;

namespace GoldEx.Calculator.Client.ViewModels;

public sealed class QuickInvoice
{
    public required string InvoiceNumber { get; init; }
    public required string DateTime { get; init; }

    public string? CustomerName { get; init; }
    public string? CustomerPhone { get; init; }

    public string? CompanyName { get; init; }
    public string? CompanyPhone { get; init; }
    public string? CompanyAddress { get; init; }

    public required List<QuickInvoiceItem> Items { get; init; } = [];

    public static QuickInvoice Create(List<QuickInvoicePayload> items)
    {
        var invoice = new QuickInvoice
        {
            InvoiceNumber = items.First().InvoiceNumber,
            DateTime = items.First().DateTime,
            CustomerName = items.First().CustomerName,
            CustomerPhone = items.First().CustomerPhone,
            CompanyName = items.First().CompanyName,
            CompanyPhone = items.First().CompanyPhone,
            CompanyAddress = items.First().CompanyAddress,
            Items = items.Select(x => x.ToItem()).ToList()
        };

        return invoice;
    }

    public List<QuickInvoicePayload> ToPayload()
    {
        return Items.Select(x => new QuickInvoicePayload
        {
            InvoiceNumber = InvoiceNumber,
            DateTime = DateTime,
            ProductType = x.ProductType,
            Weight = x.Weight,
            Fineness = x.Fineness,
            GramPrice = x.GramPrice,
            ProfitPercent = x.ProfitPercent,
            TaxPercent = x.TaxPercent,
            FinalPrice = x.FinalPrice,
            ProductName = x.ProductName,
            Wage = x.Wage,
            WageType = x.WageType,
            CompanyAddress = CompanyAddress,
            CompanyName = CompanyName,
            CompanyPhone = CompanyPhone,
            CustomerName = CustomerName,
            CustomerPhone = CustomerPhone
        }).ToList();
    }
}

public sealed class QuickInvoiceItem
{
    public required string ProductType { get; init; }
    public required string Weight { get; init; }
    public required decimal Fineness { get; init; }
    public required string GramPrice { get; init; }
    public required decimal ProfitPercent { get; init; }
    public required decimal TaxPercent { get; init; }
    public string? Wage { get; init; }
    public string? WageType { get; init; }
    public required string FinalPrice { get; init; }
    public string? ProductName { get; init; }
}

public static class QuickInvoiceMappingExtensions
{
    public static QuickInvoiceItem ToItem(this QuickInvoicePayload payload)
    {
        return new QuickInvoiceItem
        {
            ProductType = payload.ProductType,
            Weight = payload.Weight,
            Fineness = payload.Fineness,
            GramPrice = payload.GramPrice,
            ProfitPercent = payload.ProfitPercent,
            TaxPercent = payload.TaxPercent,
            Wage = payload.Wage,
            WageType = payload.WageType,
            FinalPrice = payload.FinalPrice,
            ProductName = payload.ProductName
        };
    }
}