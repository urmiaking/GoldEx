using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Helpers;

namespace GoldEx.Client.Components.Calculator.ViewModels;

public sealed record QuickInvoicePayload
{
    public required string InvoiceNumber { get; init; }
    public required string DateTime { get; init; }

    public string? CustomerName { get; init; }
    public string? CustomerPhone { get; init; }

    public string? CompanyName { get; init; }
    public string? CompanyPhone { get; init; }
    public string? CompanyAddress { get; init; }

    public string? ProductName { get; init; }

    public required string ProductType { get; init; }
    public required string Weight { get; init; }
    public required decimal Fineness { get; init; }
    public required string GramPrice { get; init; }
    public required decimal ProfitPercent { get; init; }
    public required decimal TaxPercent { get; init; }
    public string? Wage { get; init; }
    public string? WageType { get; init; }

    public required string FinalPrice { get; init; }

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static QuickInvoicePayload Create(
        CalculatorVm model,
        decimal finalPrice)
    {
        return new QuickInvoicePayload
        {
            InvoiceNumber = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            DateTime = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm"),
            CustomerName = null,
            CustomerPhone = null,
            ProductType = model.ProductType.GetDisplayName(),
            Weight = model.Weight.ToWeightFormat(model.GoldUnitType),
            Fineness = model.Fineness,
            GramPrice = model.GramPrice.ToCurrencyFormat("تومان"),
            ProfitPercent = model.ProfitPercent,
            TaxPercent = model.TaxPercent,
            Wage = model.WageType switch
            {
                Shared.Enums.WageType.Percent => model.Wage.ToString(),
                Shared.Enums.WageType.Fixed => model.Wage?.ToCurrencyFormat("تومان"),
                null => null,
                _ => throw new ArgumentOutOfRangeException()
            },
            WageType = model is { WageType: not null, Wage: not null } ? model.WageType.Value.GetDisplayName() : "ندارد",
            FinalPrice = finalPrice.ToCurrencyFormat("تومان")
        };
    }

    public QuickInvoicePayload WithCustomer(string customerName, string? customerPhone) =>
        this with
        {
            CustomerName = customerName,
            CustomerPhone = customerPhone
        };

    public QuickInvoicePayload WithCompanyInfo(string companyName, string companyPhone, string companyAddress) =>
        this with
        {
            CompanyName = companyName,
            CompanyPhone = companyPhone,
            CompanyAddress = companyAddress
        };
}