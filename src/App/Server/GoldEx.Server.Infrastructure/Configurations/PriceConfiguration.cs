using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceHistoryAggregate;
using GoldEx.Shared.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

public class PriceConfiguration : IEntityTypeConfiguration<Price>
{
    public void Configure(EntityTypeBuilder<Price> builder)
    {
        PriceBaseConfiguration.Configure<Price, PriceHistory>(builder);
    }
}