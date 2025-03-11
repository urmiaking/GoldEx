﻿// <auto-generated />
using System;
using GoldEx.Client.Offline.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GoldEx.Client.Offline.Infrastructure.Migrations
{
    [DbContext(typeof(OfflineDbContext))]
    partial class OfflineDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.2");

            modelBuilder.Entity("GoldEx.Client.Offline.Domain.CheckpointAggregate.Checkpoint", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("EntityName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Checkpoints", (string)null);
                });

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

                    b.Property<DateTime>("LastModifiedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("ProductType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Status")
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

            modelBuilder.Entity("GoldEx.Client.Offline.Domain.SettingsAggregate.Settings", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<double>("GoldProfit")
                        .HasColumnType("REAL");

                    b.Property<string>("InstitutionName")
                        .IsRequired()
                        .HasMaxLength(25)
                        .HasColumnType("TEXT");

                    b.Property<double>("JewelryProfit")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("LastModifiedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<double>("Tax")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.ToTable("Settings", (string)null);
                });

            modelBuilder.Entity("GoldEx.Client.Offline.Domain.PriceAggregate.Price", b =>
                {
                    b.OwnsOne("GoldEx.Client.Offline.Domain.PriceAggregate.PriceHistory", "PriceHistory", b1 =>
                        {
                            b1.Property<Guid>("PriceId")
                                .HasColumnType("TEXT");

                            b1.Property<double>("CurrentValue")
                                .HasColumnType("REAL");

                            b1.Property<string>("DailyChangeRate")
                                .IsRequired()
                                .HasMaxLength(50)
                                .HasColumnType("TEXT");

                            b1.Property<DateTime>("LastModifiedDate")
                                .HasColumnType("TEXT");

                            b1.Property<string>("LastUpdate")
                                .IsRequired()
                                .HasMaxLength(50)
                                .HasColumnType("TEXT");

                            b1.Property<string>("Unit")
                                .IsRequired()
                                .HasMaxLength(50)
                                .HasColumnType("TEXT");

                            b1.HasKey("PriceId");

                            b1.ToTable("PriceHistories", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("PriceId");
                        });

                    b.Navigation("PriceHistory")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
