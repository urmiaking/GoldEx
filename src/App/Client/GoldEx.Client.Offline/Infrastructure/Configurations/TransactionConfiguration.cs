using GoldEx.Client.Offline.Domain.CustomerAggregate;
using GoldEx.Client.Offline.Domain.TransactionAggregate;
using GoldEx.Shared.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Client.Offline.Infrastructure.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        TransactionBaseConfiguration.Configure<Transaction, Customer>(builder);
    }
}