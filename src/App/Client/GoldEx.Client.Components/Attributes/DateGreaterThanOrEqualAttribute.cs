using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Components.Attributes;

public class DateGreaterThanOrEqualAttribute(string comparisonProperty) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) return ValidationResult.Success;

        var property = validationContext.ObjectType.GetProperty(comparisonProperty);
        if (property is null)
            return new ValidationResult($"Unknown property: {comparisonProperty}");

        if (property.GetValue(validationContext.ObjectInstance) is DateTime comparisonValue && value is DateTime date && date < comparisonValue)
            return new ValidationResult(ErrorMessage ?? $"باید بزرگتر یا مساوی {comparisonProperty} باشد");

        return ValidationResult.Success;
    }
}
