using GoldEx.Server.Domain.OAuthAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class OAuthClientConfiguration : IEntityTypeConfiguration<OAuthClient>
{
    public void Configure(EntityTypeBuilder<OAuthClient> builder)
    {
        builder.ToTable("OAuthClients");

        builder.Property(c => c.Id)
            .HasConversion(x => x.Value, id => new OAuthClientId(id));

        builder.Property(c => c.ClientId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(c => c.ClientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ClientSecretHash)
            .HasMaxLength(128);

        builder.Property(c => c.RedirectUrisJson)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.GrantTypesJson)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(c => c.ClientId).IsUnique();
    }
}
