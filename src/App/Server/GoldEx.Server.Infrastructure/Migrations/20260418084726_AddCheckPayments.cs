using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoicePaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssuerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssuerFinancialAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SayadiCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckPayments_Customers_IssuerId",
                        column: x => x.IssuerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckPayments_FinancialAccounts_IssuerFinancialAccountId",
                        column: x => x.IssuerFinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckPayments_InvoicePayments_InvoicePaymentId",
                        column: x => x.InvoicePaymentId,
                        principalTable: "InvoicePayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CheckPaymentChangeLogs",
                columns: table => new
                {
                    CheckPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TargetFinancialAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckPaymentChangeLogs", x => new { x.CheckPaymentId, x.Id });
                    table.ForeignKey(
                        name: "FK_CheckPaymentChangeLogs_CheckPayments_CheckPaymentId",
                        column: x => x.CheckPaymentId,
                        principalTable: "CheckPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CheckPaymentChangeLogs_FinancialAccounts_TargetFinancialAccountId",
                        column: x => x.TargetFinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckPaymentChangeLogs_TargetFinancialAccountId",
                table: "CheckPaymentChangeLogs",
                column: "TargetFinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckPayments_InvoicePaymentId",
                table: "CheckPayments",
                column: "InvoicePaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckPayments_IssuerFinancialAccountId",
                table: "CheckPayments",
                column: "IssuerFinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckPayments_IssuerId",
                table: "CheckPayments",
                column: "IssuerId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckPayments_Number",
                table: "CheckPayments",
                column: "Number",
                unique: true,
                filter: "[Number] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CheckPayments_SayadiCode",
                table: "CheckPayments",
                column: "SayadiCode",
                unique: true,
                filter: "[SayadiCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckPaymentChangeLogs");

            migrationBuilder.DropTable(
                name: "CheckPayments");
        }
    }
}
