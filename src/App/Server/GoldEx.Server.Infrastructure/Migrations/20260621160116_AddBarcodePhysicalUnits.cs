using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBarcodePhysicalUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "LabelWidth",
                table: "BarcodePrintSettings",
                type: "float",
                nullable: false,
                defaultValue: 80.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 300);

            migrationBuilder.AlterColumn<double>(
                name: "LabelHeight",
                table: "BarcodePrintSettings",
                type: "float",
                nullable: false,
                defaultValue: 15.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 150);

            migrationBuilder.AlterColumn<double>(
                name: "BarcodePaddingTop",
                table: "BarcodePrintSettings",
                type: "float",
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 10);

            migrationBuilder.AlterColumn<double>(
                name: "BarcodePaddingRight",
                table: "BarcodePrintSettings",
                type: "float",
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 10);

            migrationBuilder.AlterColumn<double>(
                name: "BarcodePaddingLeft",
                table: "BarcodePrintSettings",
                type: "float",
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 10);

            migrationBuilder.AlterColumn<double>(
                name: "BarcodePaddingBottom",
                table: "BarcodePrintSettings",
                type: "float",
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 10);

            migrationBuilder.AlterColumn<double>(
                name: "BarcodeMarginTop",
                table: "BarcodePrintSettings",
                type: "float",
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 5);

            migrationBuilder.AlterColumn<double>(
                name: "BarcodeMarginRight",
                table: "BarcodePrintSettings",
                type: "float",
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 5);

            migrationBuilder.AlterColumn<double>(
                name: "BarcodeMarginLeft",
                table: "BarcodePrintSettings",
                type: "float",
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 5);

            migrationBuilder.AlterColumn<double>(
                name: "BarcodeMarginBottom",
                table: "BarcodePrintSettings",
                type: "float",
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 5);

            migrationBuilder.AddColumn<double>(
                name: "TailWidth",
                table: "BarcodePrintSettings",
                type: "float",
                nullable: false,
                defaultValue: 30.0);

            migrationBuilder.AlterColumn<double>(
                name: "ItemSpacing",
                table: "BarcodePositionItems",
                type: "float",
                nullable: false,
                defaultValue: 0.5,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 5);

            migrationBuilder.AlterColumn<double>(
                name: "FontSize",
                table: "BarcodePositionItems",
                type: "float",
                nullable: false,
                defaultValue: 7.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 12);

            migrationBuilder.AlterColumn<double>(
                name: "BarcodeWidth",
                table: "BarcodePositionItems",
                type: "float",
                nullable: true,
                defaultValue: 22.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 2);

            migrationBuilder.AlterColumn<double>(
                name: "BarcodeMargin",
                table: "BarcodePositionItems",
                type: "float",
                nullable: true,
                defaultValue: 0.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "BarcodeHeight",
                table: "BarcodePositionItems",
                type: "float",
                nullable: true,
                defaultValue: 8.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 50);

            migrationBuilder.AlterColumn<double>(
                name: "BarcodeFontSize",
                table: "BarcodePositionItems",
                type: "float",
                nullable: true,
                defaultValue: 7.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 14);

            migrationBuilder.AddColumn<int>(
                name: "BarWidthMultiplier",
                table: "BarcodePositionItems",
                type: "int",
                nullable: true,
                defaultValue: 2);

            // Data Migration: convert old pixel values to millimeter/pt equivalents
            migrationBuilder.Sql(@"
                UPDATE BarcodePrintSettings 
                SET LabelWidth = 80.0, 
                    LabelHeight = 15.0, 
                    TailWidth = 30.0,
                    BarcodeMarginTop = 1.0, 
                    BarcodeMarginRight = 1.0, 
                    BarcodeMarginBottom = 1.0, 
                    BarcodeMarginLeft = 1.0,
                    BarcodePaddingTop = 1.0, 
                    BarcodePaddingRight = 1.0, 
                    BarcodePaddingBottom = 1.0, 
                    BarcodePaddingLeft = 1.0
                WHERE LabelWidth >= 150;

                UPDATE BarcodePositionItems
                SET ItemSpacing = 0.5,
                    FontSize = 8.0,
                    BarcodeWidth = CASE WHEN BarcodeWidth IS NOT NULL THEN 22.0 ELSE BarcodeWidth END,
                    BarcodeHeight = CASE WHEN BarcodeHeight IS NOT NULL THEN 8.0 ELSE BarcodeHeight END,
                    BarcodeFontSize = CASE WHEN BarcodeFontSize IS NOT NULL THEN 7.0 ELSE BarcodeFontSize END,
                    BarcodeMargin = CASE WHEN BarcodeMargin IS NOT NULL THEN 0.0 ELSE BarcodeMargin END,
                    BarWidthMultiplier = CASE WHEN BarcodeWidth IS NOT NULL THEN 2 ELSE BarWidthMultiplier END
                WHERE FontSize >= 10;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TailWidth",
                table: "BarcodePrintSettings");

            migrationBuilder.DropColumn(
                name: "BarWidthMultiplier",
                table: "BarcodePositionItems");

            migrationBuilder.AlterColumn<int>(
                name: "LabelWidth",
                table: "BarcodePrintSettings",
                type: "int",
                nullable: false,
                defaultValue: 300,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 80.0);

            migrationBuilder.AlterColumn<int>(
                name: "LabelHeight",
                table: "BarcodePrintSettings",
                type: "int",
                nullable: false,
                defaultValue: 150,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 15.0);

            migrationBuilder.AlterColumn<int>(
                name: "BarcodePaddingTop",
                table: "BarcodePrintSettings",
                type: "int",
                nullable: false,
                defaultValue: 10,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 1.0);

            migrationBuilder.AlterColumn<int>(
                name: "BarcodePaddingRight",
                table: "BarcodePrintSettings",
                type: "int",
                nullable: false,
                defaultValue: 10,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 1.0);

            migrationBuilder.AlterColumn<int>(
                name: "BarcodePaddingLeft",
                table: "BarcodePrintSettings",
                type: "int",
                nullable: false,
                defaultValue: 10,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 1.0);

            migrationBuilder.AlterColumn<int>(
                name: "BarcodePaddingBottom",
                table: "BarcodePrintSettings",
                type: "int",
                nullable: false,
                defaultValue: 10,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 1.0);

            migrationBuilder.AlterColumn<int>(
                name: "BarcodeMarginTop",
                table: "BarcodePrintSettings",
                type: "int",
                nullable: false,
                defaultValue: 5,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 1.0);

            migrationBuilder.AlterColumn<int>(
                name: "BarcodeMarginRight",
                table: "BarcodePrintSettings",
                type: "int",
                nullable: false,
                defaultValue: 5,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 1.0);

            migrationBuilder.AlterColumn<int>(
                name: "BarcodeMarginLeft",
                table: "BarcodePrintSettings",
                type: "int",
                nullable: false,
                defaultValue: 5,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 1.0);

            migrationBuilder.AlterColumn<int>(
                name: "BarcodeMarginBottom",
                table: "BarcodePrintSettings",
                type: "int",
                nullable: false,
                defaultValue: 5,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 1.0);

            migrationBuilder.AlterColumn<int>(
                name: "ItemSpacing",
                table: "BarcodePositionItems",
                type: "int",
                nullable: false,
                defaultValue: 5,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 0.5);

            migrationBuilder.AlterColumn<int>(
                name: "FontSize",
                table: "BarcodePositionItems",
                type: "int",
                nullable: false,
                defaultValue: 12,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 7.0);

            migrationBuilder.AlterColumn<int>(
                name: "BarcodeWidth",
                table: "BarcodePositionItems",
                type: "int",
                nullable: true,
                defaultValue: 2,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true,
                oldDefaultValue: 22.0);

            migrationBuilder.AlterColumn<int>(
                name: "BarcodeMargin",
                table: "BarcodePositionItems",
                type: "int",
                nullable: true,
                defaultValue: 0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true,
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<int>(
                name: "BarcodeHeight",
                table: "BarcodePositionItems",
                type: "int",
                nullable: true,
                defaultValue: 50,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true,
                oldDefaultValue: 8.0);

            migrationBuilder.AlterColumn<int>(
                name: "BarcodeFontSize",
                table: "BarcodePositionItems",
                type: "int",
                nullable: true,
                defaultValue: 14,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true,
                oldDefaultValue: 7.0);
        }
    }
}
