using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        TransactionBaseConfiguration.Configure<Transaction, Customer>(builder);
    }
}