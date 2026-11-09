using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.CustomerTransferVoucherAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.MoltenGoldTradeAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class MoltenGoldTradeConfiguration : IEntityTypeConfiguration<MoltenGoldTrade>,
    IEntityTypeConfiguration<MoltenGoldTradeEvent>
{
    public void Configure(EntityTypeBuilder<MoltenGoldTrade> builder)
    {
        builder.ToTable("MoltenGoldTrades");
        builder.Property(x => x.CustomerId).HasConversion(x => x.Value, x => new CustomerId(x));
        builder.Property(x => x.ProductId).HasConversion(x => x!.Value.Value, x => new ProductId(x));
        builder.Property(x => x.PriceUnitId).HasConversion(x => x.Value, x => new PriceUnitId(x));
        builder.Property(x => x.GoldPriceUnitId).HasConversion(x => x!.Value.Value, x => new PriceUnitId(x));
        builder.Property(x => x.RemainingGoldBookAmount).HasPrecision(36, 10);
        builder.Property(x => x.RemainingGoldWeight).HasPrecision(36, 10);
        builder.Property(x => x.Weight).HasPrecision(36, 10);
        builder.Property(x => x.RemainingWeight).HasPrecision(36, 10);
        builder.Property(x => x.Rate).HasPrecision(36, 10);
        builder.Property(x => x.GramRate).HasPrecision(38, 18);
        builder.Property(x => x.ExchangeRate).HasPrecision(38, 18);
        builder.Property(x => x.Fineness).HasPrecision(9, 6);
        builder.Property(x => x.CommissionPercent).HasPrecision(9, 6);
        builder.Property(x => x.AssayNumber).HasMaxLength(100);
        builder.HasIndex(x => new { x.StoreId, x.ProductId, x.IsCancelled });
        builder.HasIndex(x => new { x.StoreId, x.CreatedAt });
        builder.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PriceUnit>().WithMany().HasForeignKey(x => x.PriceUnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PriceUnit>().WithMany().HasForeignKey(x => x.GoldPriceUnitId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<MoltenGoldTradeEvent> builder)
    {
        builder.ToTable("MoltenGoldTradeEvents");
        builder.Property(x => x.InvoiceId).HasConversion(x => x!.Value.Value, x => new InvoiceId(x));
        builder.Property(x => x.VoucherId).HasConversion(x => x!.Value.Value, x => new CustomerTransferVoucherId(x));
        builder.Property(x => x.Amount).HasPrecision(36, 10);
        builder.Property(x => x.Weight).HasPrecision(36, 10);
        builder.Property(x => x.Reference).HasMaxLength(250);
        builder.HasIndex(x => new { x.StoreId, x.TradeId, x.CreatedAt });
        builder.HasOne<MoltenGoldTrade>().WithMany().HasForeignKey(x => x.TradeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Invoice>().WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CustomerTransferVoucher>().WithMany().HasForeignKey(x => x.VoucherId).OnDelete(DeleteBehavior.Restrict);
    }
}
