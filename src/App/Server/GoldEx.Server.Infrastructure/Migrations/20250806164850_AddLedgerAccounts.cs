using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLedgerAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LedgerAccountId",
                table: "FinancialAccounts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LedgerAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    IsSystemAccount = table.Column<bool>(type: "bit", nullable: false),
                    ParentAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LedgerAccounts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerAccounts_LedgerAccounts_ParentAccountId",
                        column: x => x.ParentAccountId,
                        principalTable: "LedgerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_LedgerAccountId",
                table: "FinancialAccounts",
                column: "LedgerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_CustomerId",
                table: "LedgerAccounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_ParentAccountId",
                table: "LedgerAccounts",
                column: "ParentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_Title",
                table: "LedgerAccounts",
                column: "Title",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialAccounts_LedgerAccounts_LedgerAccountId",
                table: "FinancialAccounts",
                column: "LedgerAccountId",
                principalTable: "LedgerAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialAccounts_LedgerAccounts_LedgerAccountId",
                table: "FinancialAccounts");

            migrationBuilder.DropTable(
                name: "LedgerAccounts");

            migrationBuilder.DropIndex(
                name: "IX_FinancialAccounts_LedgerAccountId",
                table: "FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "LedgerAccountId",
                table: "FinancialAccounts");
        }
    }
}
