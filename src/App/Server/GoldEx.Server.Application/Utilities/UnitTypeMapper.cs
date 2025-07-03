using GoldEx.Sdk.Server.Infrastructure.DTOs;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GoldEx.Server.Application.Utilities;

public class UnitTypeMapper
{
    public static UnitType? GetUnitType(PriceResponse apiPrice)
    {
        foreach (var unitType in Enum.GetValues<UnitType>())
        {
            var displayAttribute = typeof(UnitType)
                .GetMember(unitType.ToString())[0]
                .GetCustomAttribute<DisplayAttribute>();

            if (displayAttribute != null && displayAttribute.Name == apiPrice.Title)
                return unitType;
        }

        return null;
    }
}