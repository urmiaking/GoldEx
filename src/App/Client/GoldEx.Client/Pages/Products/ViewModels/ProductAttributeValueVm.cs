using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GoldEx.Client.Pages.Products.ViewModels;

public class ProductAttributeValueVm : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _value = string.Empty;
    private decimal? _numericValue;

    public Guid AttributeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public ProductAttributeDataType DataType { get; set; } = ProductAttributeDataType.Text;
    public string? Options { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }

    public string Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                if (decimal.TryParse(value, out var n))
                    _numericValue = n;
                else
                    _numericValue = null;
                OnPropertyChanged();
            }
        }
    }

    public decimal? NumericValue
    {
        get => _numericValue;
        set
        {
            if (_numericValue != value)
            {
                _numericValue = value;
                _value = value?.ToString() ?? string.Empty;
                OnPropertyChanged();
            }
        }
    }

    public static ProductAttributeValueVm CreateFrom(ProductAttributeValueDto dto)
    {
        return new ProductAttributeValueVm
        {
            AttributeId = dto.AttributeId,
            Title = dto.Title ?? string.Empty,
            Unit = dto.Unit,
            Value = dto.Value,
            NumericValue = dto.NumericValue,
            DataType = dto.DataType
        };
    }

    public ProductAttributeValueDto ToDto()
    {
        return new ProductAttributeValueDto(
            AttributeId,
            Title,
            Unit,
            Value,
            NumericValue,
            DataType);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
