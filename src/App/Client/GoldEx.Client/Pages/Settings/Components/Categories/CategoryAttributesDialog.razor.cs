using GoldEx.Shared.DTOs.ProductAttributes;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.Categories;

public partial class CategoryAttributesDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public Guid CategoryId { get; set; }
    [Parameter] public string CategoryName { get; set; } = string.Empty;

    private bool _isLoading = true;
    private bool _isSaving;

    private List<ProductAttributeDto> _allAttributes = [];
    private List<CategoryAttributeDto> _currentAssigned = [];
    private List<CategoryAttributeRowVm> _attributeRows = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;

        var allAttrs = await SendRequestAsync<IProductAttributeService, List<ProductAttributeDto>>(
            action: (s, ct) => s.GetListAsync(ct));

        _allAttributes = allAttrs ?? [];

        var assignedAttrs = await SendRequestAsync<IProductAttributeService, List<CategoryAttributeDto>>(
            action: (s, ct) => s.GetCategoryAttributesAsync(CategoryId, ct));

        _currentAssigned = assignedAttrs ?? [];

        var assignedMap = _currentAssigned.ToDictionary(x => x.AttributeId);

        _attributeRows = _allAttributes.Select(attr =>
        {
            var isAssigned = assignedMap.TryGetValue(attr.Id, out var assigned);
            return new CategoryAttributeRowVm
            {
                AttributeId = attr.Id,
                Title = attr.Title,
                Unit = attr.Unit,
                DataType = attr.DataType,
                Options = attr.Options,
                IsSelected = isAssigned,
                IsRequired = isAssigned && assigned!.IsRequired,
                DisplayOrder = isAssigned ? assigned!.DisplayOrder : 0,
                ShowInFilter = !isAssigned || assigned!.ShowInFilter
            };
        }).OrderByDescending(x => x.IsSelected)
          .ThenBy(x => x.DisplayOrder)
          .ThenBy(x => x.Title)
          .ToList();

        _isLoading = false;
    }

    private async Task SaveAsync()
    {
        _isSaving = true;
        try
        {
            var selectedItems = _attributeRows
                .Where(x => x.IsSelected)
                .Select((item, idx) => new CategoryAttributeItemRequest(
                    item.AttributeId,
                    item.IsRequired,
                    item.DisplayOrder == 0 ? idx + 1 : item.DisplayOrder,
                    item.ShowInFilter))
                .ToList();

            var request = new SetCategoryAttributesRequest(CategoryId, selectedItems);

            await SendRequestAsync<IProductAttributeService>(
                action: (s, ct) => s.SetCategoryAttributesAsync(request, ct),
                afterSend: () =>
                {
                    AddSuccessToast("ویژگی‌های دسته‌بندی با موفقیت ذخیره شدند.");
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void Cancel() => MudDialog.Cancel();

    public class CategoryAttributeRowVm
    {
        public Guid AttributeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public ProductAttributeDataType DataType { get; set; }
        public string? Options { get; set; }
        public bool IsSelected { get; set; }
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
        public bool ShowInFilter { get; set; } = true;
    }
}
