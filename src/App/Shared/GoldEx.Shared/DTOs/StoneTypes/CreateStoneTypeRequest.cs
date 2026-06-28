using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.StoneTypes;

public record CreateStoneTypeRequest(string Title, string EnTitle, string Symbol, StoneKind Kind);
