using GoldEx.Server.Domain.OAuthAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class OAuthTokenConfiguration : IEntityTypeConfiguration<OAuthToken>
{
    public void Configure(EntityTypeBuilder<OAuthToken> builder)
    {
        builder.ToTable("OAuthTokens");

        builder.Property(c => c.Id)
            .HasConversion(x => x.Value, id => new OAuthTokenId(id));

        builder.Property(c => c.AccessTokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(c => c.RefreshTokenHash)
            .HasMaxLength(128);

        builder.Property(c => c.ClientId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(c => c.Scope)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.AccessTokenHash);
        builder.HasIndex(c => c.RefreshTokenHash);
        builder.HasIndex(c => new { c.StoreId, c.UserId });
    }
}
