using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.StoneTypes;

public record StoneTypeRequestFilter(bool? IsActive, StoneKind? StoneKind);