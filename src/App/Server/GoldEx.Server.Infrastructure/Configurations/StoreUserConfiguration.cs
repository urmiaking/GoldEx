using GoldEx.Server.Domain.StoreAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class StoreUserConfiguration : IEntityTypeConfiguration<StoreUser>
{
    public void Configure(EntityTypeBuilder<StoreUser> builder)
    {
        builder.ToTable("StoreUsers");

        builder.Property(x => x.StoreId)
            .HasConversion(id => id.Value,
                value => new StoreId(value));

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.HasKey(x => new { x.UserId, x.StoreId });
    }
}
