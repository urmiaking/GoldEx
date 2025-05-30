using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using GoldEx.Shared.DTOs.ProductCategories;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class ProductCategoryVm : IEquatable<ProductCategoryVm>
{
    public Guid Id { get; set; }

    public int Index { get; set; }

    [Display(Name = "عنوان")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string Title { get; set; } = default!;

    public static ProductCategoryVm CreateFrom(GetProductCategoryResponse response)
    {
        return new ProductCategoryVm
        {
            Id = response.Id,
            Title = response.Title,
        };
    }

    public static CreateProductCategoryRequest ToCreateRequest(ProductCategoryVm item)
    {
        return new CreateProductCategoryRequest(item.Id, item.Title);
    }

    public static UpdateProductCategoryRequest ToUpdateRequest(ProductCategoryVm item)
    {
        return new UpdateProductCategoryRequest(item.Title);
    }

    public bool Equals(ProductCategoryVm? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id) && Title == other.Title;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ProductCategoryVm)obj);
    }

    public override int GetHashCode()
    {
        return Title.GetHashCode();
    }

    public override string ToString() => Title;
}