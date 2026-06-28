using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.StoneTypes;

public record GetStoneTypeResponse(Guid Id, string Title, string EnTitle, string Symbol, StoneKind Kind, bool IsActive);
