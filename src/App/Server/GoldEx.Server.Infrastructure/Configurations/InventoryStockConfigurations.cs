using GoldEx.Server.Domain.InventoryStockAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class InventoryStockConfigurations : IEntityTypeConfiguration<InventoryStock>
{
    public void Configure(EntityTypeBuilder<InventoryStock> builder)
    {
        builder.ToTable("InventoryStocks");

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value,
                id => new InventoryStockId(id));

        builder.Property(x => x.ChangeAmount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.HasIndex(x => x.ChangeAmount);

        builder.HasIndex(x => x.ActionType);

        builder.Property(x => x.PostingDate)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(x => x.PostingDate);

        builder.HasOne(x => x.ReverseInventoryStock)
            .WithOne()
            .HasForeignKey<InventoryStock>(x => x.ReverseInventoryStockId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ReverseInventoryStockId)
            .IsUnique()
            .HasFilter("[ReverseInventoryStockId] IS NOT NULL");

        builder.HasOne(x => x.Coin)
            .WithMany()
            .HasForeignKey(x => x.CoinId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.InventoryStocks)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Invoice)
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.MeltingBatch)
            .WithMany(x => x.InventoryStocks)
            .HasForeignKey(x => x.MeltingBatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.InventoryEntry)
            .WithMany(x => x.InventoryStocks)
            .HasForeignKey(x => x.InventoryEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(x => x.MoltenGoldDetail, Configure);
    }

    private void Configure(OwnedNavigationBuilder<InventoryStock, MoltenGoldDetail> builder)
    {
        builder.ToTable("MoltenGoldDetails");

        builder.Property(x => x.AssayNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Weight)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.Fineness)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.HasOne(x => x.Assayer)
            .WithMany()
            .HasForeignKey(x => x.AssayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}