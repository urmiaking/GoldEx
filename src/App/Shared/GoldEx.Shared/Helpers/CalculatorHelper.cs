using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Helpers;

public class CalculatorHelper
{
    /// <summary>
    /// محاسبه قیمت خام طلا بر اساس وزن، عیار و قیمت گرم طلا.
    /// </summary>
    /// <param name="weight">وزن بر حسب گرم</param>
    /// <param name="gramPrice">نرخ گرم بر اساس ریال</param>
    /// <param name="caratType">عیار</param>
    /// <param name="productType">نوع محصول</param>
    /// <returns>قیمت خام طلا</returns>
    public static decimal CalculateRawPrice(decimal weight, decimal gramPrice, CaratType caratType, ProductType productType)
    {
        if (productType == ProductType.OldGold)
        {
            return weight * 735 / 750 * gramPrice;
        }

        var gramPrice24 = gramPrice / 0.75m;
        var caratRatio = GetCaratRatio(caratType);
        return weight * gramPrice24 * caratRatio;
    }

    /// <summary>
    /// محاسبه اجرت ساخت ریالی بر اساس قیمت خام، اجرت، نوع اجرت و نرخ تبدیل
    /// </summary>
    /// <param name="rawPrice">قیمت خام طلا</param>
    /// <param name="wage">اجرت</param>
    /// <param name="wageType">نوع اجرت</param>
    /// <param name="exchangeRate">نرخ تبدیل دلار</param>
    /// <returns>اجرت ساخت ریالی</returns>
    public static decimal CalculateWage(decimal rawPrice, decimal? wage, WageType? wageType, decimal? exchangeRate)
    {
        if (wage == null) return 0;

        switch (wageType)
        {
            case WageType.Percent:
                return rawPrice * (wage.Value / 100);
            //case WageType.Fixed:
            //    return wage.Value;
            case WageType.Fixed:
                if (exchangeRate == null)
                    return 0;
                return wage.Value * exchangeRate.Value;
            default:
                return 0;
        }
    }

    /// <summary>
    /// محاسبه سود فروشنده.
    /// </summary>
    /// <param name="rawPrice">قیمت خام طلا</param>
    /// <param name="wage">اجرت ساخت</param>
    /// <param name="productType">نوع محصول</param>
    /// <param name="profitPercent">سود درصدی فروشنده</param>
    /// <returns>سود فروشنده</returns>
    public static decimal CalculateProfit(decimal rawPrice, decimal wage, ProductType productType, decimal profitPercent)
    {
        if (productType is ProductType.OldGold or ProductType.MoltenGold)
            return 0;

        return (rawPrice + wage) * (profitPercent / 100);
    }

    /// <summary> 
    /// محاسبه مالیات بر ارزش افزوده.
    /// </summary>
    /// <param name="wage">اجرت ساخت</param>
    /// <param name="profit">سود فروشنده</param>
    /// <param name="taxPercent">درصد مالیات</param>
    /// <param name="productType">نوع محصول</param>
    /// <returns>مالیات بر ارزش افزوده</returns>
    public static decimal CalculateTax(decimal wage, decimal profit, decimal taxPercent, ProductType productType)
    {
        if (productType is ProductType.OldGold or ProductType.MoltenGold)
            return 0;

        return (wage + profit) * (taxPercent / 100);
    }

    /// <summary>
    /// محاسبه قیمت نهایی.
    /// </summary>
    /// <param name="rawPrice">قیمت خام طلا</param>
    /// <param name="wage">اجرت ساخت</param>
    /// <param name="profit">سود فروشنده</param>
    /// <param name="tax">مالیات بر ارزش افزوده</param>
    /// <param name="additionalPrices">مخارج اضافی</param>
    /// <param name="productType">نوع محصول</param>
    /// <returns>قیمت نهایی</returns>
    public static decimal CalculateFinalPrice(decimal rawPrice, decimal wage, decimal profit, decimal tax, decimal? additionalPrices, ProductType productType)
    {
        if (productType == ProductType.OldGold)
        {
            return rawPrice + (additionalPrices ?? 0);
        }

        return rawPrice + wage + profit + tax + (additionalPrices ?? 0);
    }

    /// <summary>
    /// دریافت نسبت عیار بر اساس نوع عیار.
    /// </summary>
    /// <param name="caratType">نوع عیار</param>
    /// <returns>نسبت عیار</returns>
    private static decimal GetCaratRatio(CaratType caratType)
    {
        return caratType switch
        {
            CaratType.Eighteen => 750m / 1000m
            ,
            CaratType.TwentyOne => 21m / 24m
            ,
            CaratType.TwentyTwo => 22m / 24m
            ,
            CaratType.TwentyFour => 1.0m
            ,
            _ => throw new ArgumentOutOfRangeException(nameof(caratType), $"Carat type '{caratType}' is not supported.")
        };
    }
}