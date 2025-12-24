using System.Linq.Expressions;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public static class InvoiceExpressions
{
    public static Expression<Func<Invoice, decimal>> TotalUnpaidAmount()
    {
        return x =>
            x.InvoiceType == InvoiceType.Sell
                ? (
                      x.ProductItems.Sum(i => i.ItemFinalAmount) +
                      x.CoinItems.Sum(c => c.ItemFinalAmount) +
                      x.CurrencyItems.Sum(c => c.ItemFinalAmount)
                      - x.UsedProducts.Sum(up => up.ItemFinalAmount)
                      - x.Discounts.Sum(d => d.Amount * (d.ExchangeRate ?? 1))
                      + x.ExtraCosts.Sum(e => e.Amount * (e.ExchangeRate ?? 1))
                  )
                  - (
                      x.InvoicePayments!
                          .Where(p => p.PaymentSide == PaymentSide.Receive)
                          .Sum(p => p.FinalAmount * (p.ExchangeRate ?? 1))
                      -
                      x.InvoicePayments!
                          .Where(p => p.PaymentSide == PaymentSide.Pay)
                          .Sum(p => p.FinalAmount * (p.ExchangeRate ?? 1))
                  )

                : (
                      x.ProductItems.Sum(i => i.ItemFinalAmount) +
                      x.CoinItems.Sum(c => c.ItemFinalAmount) +
                      x.CurrencyItems.Sum(c => c.ItemFinalAmount)
                      + x.UsedProducts.Sum(up => up.ItemFinalAmount)
                      - x.Discounts.Sum(d => d.Amount * (d.ExchangeRate ?? 1))
                      + x.ExtraCosts.Sum(e => e.Amount * (e.ExchangeRate ?? 1))
                  )
                  - (
                      x.InvoicePayments!
                          .Where(p => p.PaymentSide == PaymentSide.Pay)
                          .Sum(p => p.FinalAmount * (p.ExchangeRate ?? 1))
                      -
                      x.InvoicePayments!
                          .Where(p => p.PaymentSide == PaymentSide.Receive)
                          .Sum(p => p.FinalAmount * (p.ExchangeRate ?? 1))
                  );
    }
}