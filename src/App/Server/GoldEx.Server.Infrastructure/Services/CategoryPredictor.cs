using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Models.Ai;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;

namespace GoldEx.Server.Infrastructure.Services;

[ScopedService]
internal class CategoryPredictor(
    PredictionEnginePool<CategoryModelInput, CategoryModelOutput> pool,
    ILogger<CategoryPredictor> logger) : ICategoryPredictor
{
    public string Predict(CategoryModelInput input)
    {
        try
        {
            using var engine = pool.GetPredictionEngine();
            var result = engine.Predict(input);

            return result.PredictedLabel;

        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            throw;
        }
    }
}