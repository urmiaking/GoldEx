using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        ProductBaseConfiguration.Configure(builder);

        builder.HasOne(x => x.CreatedUser)
            .WithMany()
            .IsRequired()
            .HasForeignKey(x => x.CreatedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}