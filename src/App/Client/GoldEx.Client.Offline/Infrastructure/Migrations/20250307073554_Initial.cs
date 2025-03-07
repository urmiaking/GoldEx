using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Client.Offline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Checkpoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IconFile = table.Column<string>(type: "TEXT", nullable: true),
                    MarketType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Barcode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Weight = table.Column<double>(type: "REAL", nullable: false),
                    Wage = table.Column<double>(type: "REAL", nullable: true),
                    ProductType = table.Column<int>(type: "INTEGER", nullable: false),
                    CaratType = table.Column<int>(type: "INTEGER", nullable: false),
                    WageType = table.Column<int>(type: "INTEGER", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceHistories",
                columns: table => new
                {
                    PriceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurrentValue = table.Column<double>(type: "REAL", nullable: false),
                    LastUpdate = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DailyChangeRate = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistories", x => x.PriceId);
                    table.ForeignKey(
                        name: "FK_PriceHistories_Prices_PriceId",
                        column: x => x.PriceId,
                        principalTable: "Prices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Checkpoints");

            migrationBuilder.DropTable(
                name: "PriceHistories");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Prices");
        }
    }
}
