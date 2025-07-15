using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using GoldEx.Sdk.Common.Extensions;

namespace GoldEx.Client.Pages.Products.ViewModels;

public class ProductVm : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private decimal? _weight;
    private decimal? _wage;
    private WageType? _wageType;
    private ProductType _productType;
    private CaratType _caratType;
    private string? _name;
    private string _barcode = default!;

    [Display(Name = "نام جنس")]
    [Required(ErrorMessage = "نام جنس الزامی است")]
    public string? Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    [Display(Name = "بارکد")]
    [Required(ErrorMessage = "بارکد الزامی است")]
    public string Barcode
    {
        get => _barcode;
        set
        {
            if (_barcode != value)
            {
                _barcode = value;
                OnPropertyChanged();
            }
        }
    }

    [Display(Name = "وزن")]
    [Required(ErrorMessage = "وزن الزامی است")]
    public decimal? Weight
    {
        get => _weight;
        set
        {
            if (_weight != value)
            {
                _weight = value;
                OnPropertyChanged(); 
            }
        }
    }

    [Display(Name = "اجرت")]
    public decimal? Wage
    {
        get => _wage;
        set
        {
            if (_wage != value)
            {
                _wage = value;
                OnPropertyChanged();
            }
        }
    }

    [Display(Name = "نوع اجرت")]
    public WageType? WageType
    {
        get => _wageType;
        set
        {
            if (_wageType != value)
            {
                _wageType = value;
                OnPropertyChanged();
            }
        }
    }

    [Display(Name = "نوع جنس")]
    public ProductType ProductType
    {
        get => _productType;
        set
        {
            if (_productType != value)
            {
                _productType = value;
                OnPropertyChanged();
            }
        }
    }

    [Display(Name = "عیار")]
    public CaratType CaratType
    {
        get => _caratType;
        set
        {
            if (_caratType != value)
            {
                _caratType = value;
                OnPropertyChanged();
            }
        }
    }

    public Guid? Id { get; set; }
    [Display(Name = "دسته بندی")]
    public Guid? ProductCategoryId { get; set; }
    public string? ProductCategoryTitle { get; set; } = string.Empty;
    [Display(Name = "واحد قیمت اجرت")]
    public Guid? WagePriceUnitId { get; set; }
    public string? WagePriceUnitTitle { get; set; }
    public ProductCategoryVm? CategoryVm { get; set; }
    public List<GemStoneVm>? Stones { get; set; }
    public Guid? InvoiceId { get; set; }
    public DateTime DateTime { get; set; }
    public GoldUnitType GoldUnitType { get; set; }

    internal static ProductVm CreateDefaultInstance() => new()
    { CaratType = CaratType.Eighteen, ProductType = ProductType.Gold, WageType = Shared.Enums.WageType.Percent };

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal static ProductVm CreateFrom(GetProductResponse item)
    {
        return new ProductVm
        {
            Id = item.Id,
            Name = item.Name,
            Barcode = item.Barcode,
            Weight = item.Weight,
            Wage = item.Wage,
            WageType = item.WageType,
            ProductType = item.ProductType,
            CaratType = item.CaratType,
            ProductCategoryId = item.ProductCategoryId,
            ProductCategoryTitle = item.ProductCategoryTitle,
            WagePriceUnitId = item.WagePriceUnitId,
            WagePriceUnitTitle = item.WagePriceUnitTitle,
            InvoiceId = item.InvoiceId,
            DateTime = item.DateTime,
            GoldUnitType = item.GoldUnitType,
            CategoryVm = item.ProductCategoryId.HasValue && !string.IsNullOrEmpty(item.ProductCategoryTitle) ? new ProductCategoryVm
            {
                Id = item.ProductCategoryId.Value,
                Title = item.ProductCategoryTitle
            } : null,
            Stones = item.GemStones?.Select(x => new GemStoneVm
            {
                Type = x.Type,
                Carat = x.Carat,
                Code = x.Code,
                Color = x.Color,
                Cut = x.Cut,
                Purity = x.Purity
            }).ToList()
        };
    }

    internal static ProductRequestDto ToRequest(ProductVm item)
    {
        return new ProductRequestDto
        (
            item.Id,
            item.Name ?? string.Empty,
            item.Barcode,
            item.Weight ?? 0,
            item.Wage ?? 0,
            item.WageType!.Value,
            item.ProductType,
            item.CaratType,
            item.GoldUnitType,
            item.ProductCategoryId,
            item.WagePriceUnitId,
            item.Stones?.Select(x => new GemStoneRequestDto(
                    x.Code,
                    x.Type,
                    x.Color,
                    x.Cut,
                    x.Carat,
                    x.Purity))
                .ToList()
        );
    }

    public static ProductVm CreateFromSearch(GetProductResponse item)
    {
        return new ProductVm
        {
            Name = item.Name,
            Weight = item.Weight,
            Wage = item.Wage,
            WageType = item.WageType,
            ProductType = item.ProductType,
            CaratType = item.CaratType,
            ProductCategoryId = item.ProductCategoryId,
            ProductCategoryTitle = item.ProductCategoryTitle,
            WagePriceUnitId = item.WagePriceUnitId,
            WagePriceUnitTitle = item.WagePriceUnitTitle,
            CategoryVm = item.ProductCategoryId.HasValue && !string.IsNullOrEmpty(item.ProductCategoryTitle)
                ? new ProductCategoryVm
                {
                    Id = item.ProductCategoryId.Value,
                    Title = item.ProductCategoryTitle
                }
                : null,
            Barcode = StringExtensions.GenerateRandomBarcode()
        };
    }
}