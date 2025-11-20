using Microsoft.ML.Data;

namespace GoldEx.Server.Infrastructure.Models.Ai;

public class CategoryModelOutput
{
    [ColumnName(@"PredictedLabel")]
    public string PredictedLabel { get; set; } = null!;
}