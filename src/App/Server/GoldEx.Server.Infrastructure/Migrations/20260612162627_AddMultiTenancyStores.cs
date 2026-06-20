using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancyStores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SmsTemplates_Subject",
                table: "SmsTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Products_Barcode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_PrefixCode",
                table: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_Title",
                table: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_PaymentVouchers_VoucherNumber",
                table: "PaymentVouchers");

            migrationBuilder.DropIndex(
                name: "IX_LedgerAccounts_CustomerId_ParentAccountId_PriceUnitId",
                table: "LedgerAccounts");

            migrationBuilder.DropIndex(
                name: "IX_LedgerAccounts_Title",
                table: "LedgerAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_InvoiceNumber_InvoiceType",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Customers_NationalId_CustomerType",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CoinInstances_Barcode",
                table: "CoinInstances");

            migrationBuilder.DropIndex(
                name: "IX_CheckPayments_Number",
                table: "CheckPayments");

            migrationBuilder.DropIndex(
                name: "IX_CheckPayments_SayadiCode",
                table: "CheckPayments");

            migrationBuilder.DropIndex(
                name: "IX_BarcodeReservations_Barcode",
                table: "BarcodeReservations");

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "SmsTemplates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "SmsLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Settings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "ProductCategories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "PriceUnits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "PaymentVouchers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "MeltingBatches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "LedgerAccounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "InvoicePayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "InventoryStocks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "InventoryExits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "InventoryEntries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "FinancialAccounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Coins",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "CoinInstances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "CheckPayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "BarcodeReservations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "BarcodeInquiries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "AppLicenses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "AppLicensePayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BackgroundImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoreUsers",
                columns: table => new
                {
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreUsers", x => new { x.UserId, x.StoreId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplates_StoreId_Subject",
                table: "SmsTemplates",
                columns: new[] { "StoreId", "Subject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_StoreId_Barcode",
                table: "Products",
                columns: new[] { "StoreId", "Barcode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_StoreId_PrefixCode",
                table: "ProductCategories",
                columns: new[] { "StoreId", "PrefixCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_StoreId_Title",
                table: "ProductCategories",
                columns: new[] { "StoreId", "Title" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentVouchers_StoreId_VoucherNumber",
                table: "PaymentVouchers",
                columns: new[] { "StoreId", "VoucherNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_CustomerId",
                table: "LedgerAccounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_StoreId_CustomerId_ParentAccountId_PriceUnitId",
                table: "LedgerAccounts",
                columns: new[] { "StoreId", "CustomerId", "ParentAccountId", "PriceUnitId" },
                unique: true,
                filter: "[CustomerId] IS NOT NULL AND [ParentAccountId] IS NOT NULL AND [PriceUnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_StoreId_Title",
                table: "LedgerAccounts",
                columns: new[] { "StoreId", "Title" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_StoreId_InvoiceNumber_InvoiceType",
                table: "Invoices",
                columns: new[] { "StoreId", "InvoiceNumber", "InvoiceType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_StoreId_NationalId_CustomerType",
                table: "Customers",
                columns: new[] { "StoreId", "NationalId", "CustomerType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoinInstances_StoreId_Barcode",
                table: "CoinInstances",
                columns: new[] { "StoreId", "Barcode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckPayments_StoreId_Number",
                table: "CheckPayments",
                columns: new[] { "StoreId", "Number" },
                unique: true,
                filter: "[Number] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CheckPayments_StoreId_SayadiCode",
                table: "CheckPayments",
                columns: new[] { "StoreId", "SayadiCode" },
                unique: true,
                filter: "[SayadiCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeReservations_StoreId_Barcode",
                table: "BarcodeReservations",
                columns: new[] { "StoreId", "Barcode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Slug",
                table: "Stores",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "StoreUsers");

            migrationBuilder.DropIndex(
                name: "IX_SmsTemplates_StoreId_Subject",
                table: "SmsTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Products_StoreId_Barcode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_StoreId_PrefixCode",
                table: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_StoreId_Title",
                table: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_PaymentVouchers_StoreId_VoucherNumber",
                table: "PaymentVouchers");

            migrationBuilder.DropIndex(
                name: "IX_LedgerAccounts_CustomerId",
                table: "LedgerAccounts");

            migrationBuilder.DropIndex(
                name: "IX_LedgerAccounts_StoreId_CustomerId_ParentAccountId_PriceUnitId",
                table: "LedgerAccounts");

            migrationBuilder.DropIndex(
                name: "IX_LedgerAccounts_StoreId_Title",
                table: "LedgerAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_StoreId_InvoiceNumber_InvoiceType",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Customers_StoreId_NationalId_CustomerType",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CoinInstances_StoreId_Barcode",
                table: "CoinInstances");

            migrationBuilder.DropIndex(
                name: "IX_CheckPayments_StoreId_Number",
                table: "CheckPayments");

            migrationBuilder.DropIndex(
                name: "IX_CheckPayments_StoreId_SayadiCode",
                table: "CheckPayments");

            migrationBuilder.DropIndex(
                name: "IX_BarcodeReservations_StoreId_Barcode",
                table: "BarcodeReservations");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "SmsTemplates");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "SmsLogs");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "PriceUnits");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "PaymentVouchers");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "MeltingBatches");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "LedgerAccounts");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "InventoryExits");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "InventoryEntries");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Coins");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "CoinInstances");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "CheckPayments");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "BarcodeReservations");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "BarcodeInquiries");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "AppLicenses");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "AppLicensePayments");

            migrationBuilder.CreateIndex(
                name: "IX_SmsTemplates_Subject",
                table: "SmsTemplates",
                column: "Subject",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_PrefixCode",
                table: "ProductCategories",
                column: "PrefixCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Title",
                table: "ProductCategories",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentVouchers_VoucherNumber",
                table: "PaymentVouchers",
                column: "VoucherNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_CustomerId_ParentAccountId_PriceUnitId",
                table: "LedgerAccounts",
                columns: new[] { "CustomerId", "ParentAccountId", "PriceUnitId" },
                unique: true,
                filter: "[CustomerId] IS NOT NULL AND [ParentAccountId] IS NOT NULL AND [PriceUnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_Title",
                table: "LedgerAccounts",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber_InvoiceType",
                table: "Invoices",
                columns: new[] { "InvoiceNumber", "InvoiceType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NationalId_CustomerType",
                table: "Customers",
                columns: new[] { "NationalId", "CustomerType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoinInstances_Barcode",
                table: "CoinInstances",
                column: "Barcode",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeReservations_Barcode",
                table: "BarcodeReservations",
                column: "Barcode",
                unique: true);
        }
    }
}
