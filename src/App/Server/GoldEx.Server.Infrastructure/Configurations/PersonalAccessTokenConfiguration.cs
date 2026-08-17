using GoldEx.Server.Domain.PersonalAccessTokenAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class PersonalAccessTokenConfiguration : IEntityTypeConfiguration<PersonalAccessToken>
{
    public void Configure(EntityTypeBuilder<PersonalAccessToken> builder)
    {
        builder.ToTable("PersonalAccessTokens");

        builder.Property(c => c.Id)
            .HasConversion(x => x.Value,
                id => new PersonalAccessTokenId(id));

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(c => c.TokenPrefix)
            .IsRequired()
            .HasMaxLength(32);

        builder.HasIndex(c => c.TokenHash);
        builder.HasIndex(c => new { c.StoreId, c.UserId });
    }
}
