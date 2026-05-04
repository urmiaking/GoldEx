using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Components.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class RequiredIfAttribute(string dependentProperty, object targetValue) : ValidationAttribute
{
    public string DependentProperty { get; } = dependentProperty;
    public object TargetValue { get; } = targetValue;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var propertyInfo = validationContext.ObjectType.GetProperty(DependentProperty);
        if (propertyInfo == null)
        {
            return new ValidationResult($"Property '{DependentProperty}' not found.");
        }

        var dependentValue = propertyInfo.GetValue(validationContext.ObjectInstance);

        if (Equals(dependentValue, TargetValue))
        {
            var isEmpty = value == null ||
                           (value is string str && string.IsNullOrWhiteSpace(str)) ||
                           (value is Guid guid && guid == Guid.Empty);

            if (isEmpty)
            {
                var errorMessage = ErrorMessage ?? $"فیلد {validationContext.DisplayName} الزامی است.";
                return new ValidationResult(errorMessage, [validationContext.MemberName!]);
            }
        }

        return ValidationResult.Success;
    }
}