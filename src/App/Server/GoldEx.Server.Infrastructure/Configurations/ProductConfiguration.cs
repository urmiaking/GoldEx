﻿using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Shared.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        ProductBaseConfiguration.Configure<Product, ProductCategory, GemStone>(builder);

        builder.HasOne(x => x.CreatedUser)
            .WithMany()
            .IsRequired()
            .HasForeignKey(x => x.CreatedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}