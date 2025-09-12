using GoldEx.Shared.DTOs.ProductCategories;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class ProductCategoryVm : IEquatable<ProductCategoryVm>
{
    public Guid Id { get; set; }

    public int Index { get; set; }

    [Display(Name = "عنوان")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string Title { get; set; } = default!;

    [Display(Name = "پیش کد")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string PrefixCode { get; set; } = default!;

    public static ProductCategoryVm CreateFrom(GetProductCategoryResponse response)
    {
        return new ProductCategoryVm
        {
            Id = response.Id,
            Title = response.Title,
            PrefixCode = response.PrefixCode
        };
    }

    public static CreateProductCategoryRequest ToCreateRequest(ProductCategoryVm item)
    {
        return new CreateProductCategoryRequest(item.Title, item.PrefixCode);
    }

    public static UpdateProductCategoryRequest ToUpdateRequest(ProductCategoryVm item)
    {
        return new UpdateProductCategoryRequest(item.Title, item.PrefixCode);
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