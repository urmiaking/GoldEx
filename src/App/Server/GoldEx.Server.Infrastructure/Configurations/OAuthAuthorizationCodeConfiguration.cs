using GoldEx.Server.Domain.OAuthAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class OAuthAuthorizationCodeConfiguration : IEntityTypeConfiguration<OAuthAuthorizationCode>
{
    public void Configure(EntityTypeBuilder<OAuthAuthorizationCode> builder)
    {
        builder.ToTable("OAuthAuthorizationCodes");

        builder.Property(c => c.Id)
            .HasConversion(x => x.Value, id => new OAuthAuthorizationCodeId(id));

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(c => c.ClientId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(c => c.RedirectUri)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(c => c.Scope)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.CodeChallenge)
            .HasMaxLength(256);

        builder.Property(c => c.CodeChallengeMethod)
            .HasMaxLength(32);

        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => new { c.StoreId, c.UserId });
    }
}
