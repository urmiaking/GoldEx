using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Shared.DTOs.Categories;

public record GetPendingCategoryResponse(Guid Id, string Title, ModifyStatus? Status, bool? IsDeleted);