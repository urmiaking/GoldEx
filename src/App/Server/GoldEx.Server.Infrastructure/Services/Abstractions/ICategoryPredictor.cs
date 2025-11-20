using GoldEx.Server.Infrastructure.Models.Ai;

namespace GoldEx.Server.Infrastructure.Services.Abstractions;

public interface ICategoryPredictor
{
    string Predict(CategoryModelInput input);
}