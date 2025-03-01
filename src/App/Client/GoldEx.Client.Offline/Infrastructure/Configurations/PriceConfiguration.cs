using GoldEx.Client.Offline.Domain.PriceAggregate;
using GoldEx.Client.Offline.Domain.PriceHistoryAggregate;
using GoldEx.Shared.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Client.Offline.Infrastructure.Configurations;

public class PriceConfiguration : IEntityTypeConfiguration<Price>
{
    public void Configure(EntityTypeBuilder<Price> builder)
    {
        PriceBaseConfiguration.Configure<Price, PriceHistory>(builder);
    }
}