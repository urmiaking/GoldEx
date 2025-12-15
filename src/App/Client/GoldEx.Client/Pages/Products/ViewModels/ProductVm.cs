using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace GoldEx.Client.Pages.Products.ViewModels;

public class ProductVm : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private decimal? _weight;
    private decimal? _wage;
    private WageType? _wageType;
    private ProductType _productType;
    private decimal _fineness;
    private string? _name;
    private string? _barcode;
    private ObservableCollection<GemStoneVm>? _stones;

    [Display(Name = "عنوان جنس")]
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

    [Display(Name = "بارکد/سریال")]
    public string? Barcode
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
    public decimal Fineness
    {
        get => _fineness;
        set
        {
            if (_fineness != value)
            {
                _fineness = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<GemStoneVm>? Stones
    {
        get => _stones;
        set
        {
            if (_stones != value)
            {
                // Unsubscribe from old list to prevent memory leaks
                if (_stones != null)
                {
                    _stones.CollectionChanged -= OnStonesCollectionChanged;
                    foreach (var stone in _stones) stone.PropertyChanged -= OnStoneChanged;
                }

                _stones = value;

                // Subscribe to new list
                if (_stones != null)
                {
                    _stones.CollectionChanged += OnStonesCollectionChanged;
                    foreach (var stone in _stones) stone.PropertyChanged += OnStoneChanged;
                }

                OnPropertyChanged();
            }
        }
    }

    public Guid? Id { get; set; }

    [Display(Name = "دسته بندی")]
    public Guid? ProductCategoryId { get; set; }
    public string? ProductCategoryTitle { get; set; }

    [Display(Name = "واحد قیمت اجرت")]
    public Guid? WagePriceUnitId { get; set; }
    public string? WagePriceUnitTitle { get; set; }

    [Display(Name = "واحد قیمت سنگ")]
    public GetPriceUnitTitleResponse? StonePriceUnit { get; set; }

    public ProductCategoryVm? CategoryVm { get; set; }
    public DateTime DateTime { get; set; }
    public GoldUnitType GoldUnitType { get; set; }
    public MoltenGoldVm? MoltenGold { get; set; }

    internal static ProductVm CreateDefaultInstance() => new()
    { Fineness = 750m, ProductType = ProductType.Gold, WageType = Shared.Enums.WageType.Percent };

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnStoneChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Stones));
    }

    private void OnStonesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (GemStoneVm item in e.NewItems)
                item.PropertyChanged += OnStoneChanged;
        }

        if (e.OldItems != null)
        {
            foreach (GemStoneVm item in e.OldItems)
                item.PropertyChanged -= OnStoneChanged;
        }

        OnPropertyChanged(nameof(Stones));
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
            Fineness = item.Fineness,
            ProductCategoryId = item.ProductCategoryId,
            ProductCategoryTitle = item.ProductCategoryTitle,
            WagePriceUnitId = item.WagePriceUnitId,
            WagePriceUnitTitle = item.WagePriceUnitTitle,
            StonePriceUnit = item.StonePriceUnit,
            DateTime = item.DateTime,
            GoldUnitType = item.GoldUnitType,
            CategoryVm = item.ProductCategoryId.HasValue && !string.IsNullOrEmpty(item.ProductCategoryTitle) ? new ProductCategoryVm
            {
                Id = item.ProductCategoryId.Value,
                Title = item.ProductCategoryTitle
            } : null,
            Stones = item.GemStones != null
                ? new ObservableCollection<GemStoneVm>(item.GemStones.Select(GemStoneVm.CreateFrom)) : [],
            MoltenGold = item.ProductType is ProductType.MoltenGold ? MoltenGoldVm.CreateFrom(item.MoltenGold) : null
        };
    }

    internal static ProductVm CreateFromInvoice(GetInvoiceProductItemResponse response)
    {
        var vm = CreateFrom(response.Product);

        if (response.SaleWage is not null)
        {
            vm.Wage = response.SaleWage;
            vm.WageType = response.SaleWageType;
            vm.WagePriceUnitId = response.SaleWagePriceUnitId;
            vm.WagePriceUnitTitle = response.SaleWagePriceUnitTitle;
        }

        return vm;
    }

    internal static ProductRequestDto ToRequest(ProductVm item)
    {
        return new ProductRequestDto
        (
            item.Id,
            item.Name,
            item.Barcode,
            item.Weight ?? 0,
            item.Wage ?? 0,
            item.WageType,
            item.ProductType,
            item.Fineness,
            item.GoldUnitType,
            item.ProductCategoryId,
            item.WagePriceUnitId,
            item.StonePriceUnit?.Id,
            item.Stones?.Select(x => x.ToRequest()).ToList(),
            item.MoltenGold?.ToRequest()
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
            Fineness = item.Fineness,
            ProductCategoryId = item.ProductCategoryId,
            ProductCategoryTitle = item.ProductCategoryTitle,
            WagePriceUnitId = item.WagePriceUnitId,
            WagePriceUnitTitle = item.WagePriceUnitTitle,
            StonePriceUnit = item.StonePriceUnit,
            CategoryVm = item.ProductCategoryId.HasValue && !string.IsNullOrEmpty(item.ProductCategoryTitle)
                ? new ProductCategoryVm
                {
                    Id = item.ProductCategoryId.Value,
                    Title = item.ProductCategoryTitle
                }
                : null,
            Stones = item.GemStones != null
                ? new ObservableCollection<GemStoneVm>(item.GemStones.Select(GemStoneVm.CreateFrom))
                : [],
            MoltenGold = item.ProductType is ProductType.MoltenGold ? MoltenGoldVm.CreateFrom(item.MoltenGold) : null
        };
    }

    public ProductVm Clone()
    {
        return (ProductVm)MemberwiseClone();
    }
}