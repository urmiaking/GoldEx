using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBarcodeReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BarcodeReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarcodeReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BarcodeReservations_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeReservations_Barcode",
                table: "BarcodeReservations",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeReservations_InvoiceId",
                table: "BarcodeReservations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeReservations_Prefix",
                table: "BarcodeReservations",
                column: "Prefix");

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeReservations_Prefix_Status_ExpiresAt",
                table: "BarcodeReservations",
                columns: new[] { "Prefix", "Status", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BarcodeReservations");
        }
    }
}
