using Microsoft.ML.Data;

namespace GoldEx.Server.Infrastructure.Models.Ai;

public class CategoryModelInput
{
    [LoadColumn(0)]
    [ColumnName("کد")]
    public required float Barcode { get; set; }

    [LoadColumn(1)]
    [ColumnName("نوع کالا")]
    public required string ProductType { get; set; } = null!;

    [LoadColumn(2)]
    [ColumnName("نام کالا")]
    public required string Name { get; set; } = null!;

    [LoadColumn(3)]
    [ColumnName("وزن")]
    public required float Weight { get; set; }

    [LoadColumn(4)]
    [ColumnName("اجرت")]
    public required float Wage { get; set; }

    [LoadColumn(5)]
    [ColumnName("نوع اجرت")]
    public required string WageType { get; set; } = null!;

    [LoadColumn(6)]
    [ColumnName("عیار")]
    public required float Fineness { get; set; }

    [LoadColumn(7)]
    [ColumnName("دسته بندی")]
    public string Category { get; set; } = string.Empty;
}