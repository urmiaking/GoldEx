using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum ProductAttributeDataType
{
    [Display(Name = "متنی (Text)")]
    Text = 1,

    [Display(Name = "عددی (Number)")]
    Number = 2,

    [Display(Name = "انتخابی (Dropdown Select)")]
    Select = 3
}
