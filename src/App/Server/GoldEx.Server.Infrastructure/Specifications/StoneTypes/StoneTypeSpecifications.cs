using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.StoneTypeAggregate;
using GoldEx.Shared.DTOs.StoneTypes;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.StoneTypes;

public class StoneTypesByIdSpecification(StoneTypeId id) : SpecificationBase<StoneType>(x => x.Id == id);

public class StoneTypesByTitleSpecification(string title) : SpecificationBase<StoneType>(x => x.Title == title);

public class StoneTypesByEnTitleSpecification(string enTitle) : SpecificationBase<StoneType>(x => x.EnTitle == enTitle);

public class StoneTypesBySymbolSpecification(string symbol) : SpecificationBase<StoneType>(x => x.Symbol == symbol);

public class StoneTypesByKindSpecification(StoneKind kind) : SpecificationBase<StoneType>(x => x.Kind == kind);

public class StoneTypesDefaultSpecification : SpecificationBase<StoneType>;

public class StoneTypesByFilterSpecification : SpecificationBase<StoneType>
{
    public StoneTypesByFilterSpecification(RequestFilter filter, StoneTypeRequestFilter requestFilter)
    {
        if (requestFilter.IsActive.HasValue)
        {
            AddCriteria(x => x.IsActive == requestFilter.IsActive.Value);
        }

        if (requestFilter.StoneKind.HasValue)
        {
            AddCriteria(x => x.Kind == requestFilter.StoneKind.Value);
        }

        if (!string.IsNullOrEmpty(filter.Search))
        {
            AddCriteria(x =>
                x.Title.Contains(filter.Search) ||
                x.EnTitle.Contains(filter.Search) ||
                x.Symbol.Contains(filter.Search));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection != null && filter.SortDirection != SortDirection.None)
        {
            ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(StoneType.CreatedAt), SortDirection.Ascending);
        }

        // --- Paging ---
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 15;
        ApplyPaging(skip, take);
    }
}