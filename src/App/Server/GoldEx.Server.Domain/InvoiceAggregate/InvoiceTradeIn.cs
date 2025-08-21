using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Helpers;

namespace GoldEx.Server.Domain.InvoiceAggregate;

public class InvoiceTradeIn : EntityBase
{
    internal static InvoiceTradeIn Create(string description,
        decimal weight,
        decimal gramPrice,
        int fineness,
        bool isSellable,
        ProductId? resultingProductId,
        Invoice invoice)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(fineness, 0, nameof(fineness));
        ArgumentOutOfRangeException.ThrowIfLessThan(weight, 0, nameof(weight));
        ArgumentOutOfRangeException.ThrowIfLessThan(gramPrice, 0, nameof(gramPrice));

        if (!isSellable && resultingProductId.HasValue)
            throw new ArgumentException("If the trade-in is not sellable, resulting product ID should not be provided.",
                nameof(isSellable));

        return new InvoiceTradeIn
        {
            Description = description,
            Fineness = fineness,
            Weight = weight,
            GramPrice = gramPrice,
            IsSellable = isSellable,
            ResultingProductId = resultingProductId,
            Invoice = invoice,
            ItemFinalAmount = CalculatorHelper.UsedGolds.Calculate(weight, fineness, gramPrice, invoice.ExchangeRate)
        };
    }

    public string Description { get; private set; }
    public int Fineness { get; private set; }
    public decimal Weight { get; private set; }
    public decimal GramPrice { get; private set; }
    public bool IsSellable { get; private set; }

    public ProductId? ResultingProductId { get; private set; }
    public Product? ResultingProduct { get; private set; }

    public InvoiceId InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; } = null!;

    public decimal ItemFinalAmount { get; private set; }

#pragma warning disable CS8618 
    private InvoiceTradeIn() { }
#pragma warning restore CS8618
}