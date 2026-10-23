using GoldEx.Server.Domain.Reporting;

namespace GoldEx.Server.Infrastructure.Models;

public record ProfitAnalysisData(List<GoldProfitSource> Lines, HashSet<Guid> ProductsWithOtherMovements);
