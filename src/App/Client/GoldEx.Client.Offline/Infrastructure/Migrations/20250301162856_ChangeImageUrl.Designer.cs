﻿// <auto-generated />
using System;
using GoldEx.Client.Offline.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GoldEx.Client.Offline.Infrastructure.Migrations
{
    [DbContext(typeof(OfflineDbContext))]
    [Migration("20250301162856_ChangeImageUrl")]
    partial class ChangeImageUrl
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.2");

            modelBuilder.Entity("GoldEx.Client.Offline.Domain.PriceAggregate.Price", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("IconFile")
                        .HasColumnType("TEXT");

                    b.Property<int>("MarketType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Prices", (string)null);
                });

            modelBuilder.Entity("GoldEx.Client.Offline.Domain.PriceHistoryAggregate.PriceHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("CurrentValue")
                        .HasColumnType("REAL");

                    b.Property<string>("DailyChangeRate")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastUpdate")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<Guid>("PriceId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PriceId");

                    b.ToTable("PriceHistories", (string)null);
                });

            modelBuilder.Entity("GoldEx.Client.Offline.Domain.ProductAggregate.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Barcode")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("CaratType")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("ProductType")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Synced")
                        .HasColumnType("INTEGER");

                    b.Property<double?>("Wage")
                        .HasColumnType("REAL");

                    b.Property<int?>("WageType")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Weight")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.ToTable("Products", (string)null);
                });

            modelBuilder.Entity("GoldEx.Client.Offline.Domain.PriceHistoryAggregate.PriceHistory", b =>
                {
                    b.HasOne("GoldEx.Client.Offline.Domain.PriceAggregate.Price", null)
                        .WithMany("PriceHistories")
                        .HasForeignKey("PriceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("GoldEx.Client.Offline.Domain.PriceAggregate.Price", b =>
                {
                    b.Navigation("PriceHistories");
                });
#pragma warning restore 612, 618
        }
    }
}
