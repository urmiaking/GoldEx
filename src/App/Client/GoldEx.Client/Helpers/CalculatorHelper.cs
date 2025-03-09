using GoldEx.Client.Pages.Calculate.ViewModels;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Helpers;

public class CalculatorHelper
{
    /// <summary>
    /// محاسبه قیمت خام طلا بر اساس وزن، عیار و قیمت گرم طلا.
    /// </summary>
    /// <param name="model">مدل محاسباتی</param>
    /// <returns>قیمت خام طلا</returns>
    public static double CalculateRawPrice(CalculatorVm model)
    {
        if (model.ProductType == ProductType.UsedGold)
        {
            return model.Weight * 735 / 750 * model.GramPrice;
        }

        var gramPrice24 = model.GramPrice / 0.75;
        var caratRatio = GetCaratRatio(model.CaratType);
        return model.Weight * gramPrice24 * caratRatio;
    }

    /// <summary>
    /// محاسبه اجرت ساخت بر اساس نوع اجرت و اطلاعات مدل.
    /// </summary>
    /// <param name="model">مدل محاسباتی</param>
    /// <param name="rawPrice">قیمت خام طلا</param>
    /// <returns>اجرت ساخت</returns>
    public static double CalculateWage(CalculatorVm model, double rawPrice)
    {
        if (model.Wage == null) return 0;

        switch (model.WageType)
        {
            case WageType.Percent:
                return rawPrice * (model.Wage.Value / 100);
            case WageType.Rial:
                return model.Wage.Value;
            case WageType.Dollar:
                if (model.UsDollarPrice == null)
                    return 0;
                return model.Wage.Value * model.UsDollarPrice.Value;
            default:
                return 0;
        }
    }

    /// <summary>
    /// محاسبه سود فروشنده.
    /// </summary>
    /// <param name="model">مدل محاسباتی</param>
    /// <param name="rawPrice">قیمت خام طلا</param>
    /// <param name="wage">اجرت ساخت</param>
    /// <returns>سود فروشنده</returns>
    public static double CalculateProfit(CalculatorVm model, double rawPrice, double wage)
    {
        if (model.ProductType is ProductType.UsedGold or ProductType.MoltenGold)
            return 0;

        return (rawPrice + wage) * (model.Profit / 100);
    }

    /// <summary>
    /// محاسبه مالیات بر ارزش افزوده.
    /// </summary>
    /// <param name="model">مدل محاسباتی</param>
    /// <param name="wage">اجرت ساخت</param>
    /// <param name="profit">سود فروشنده</param>
    /// <returns>مالیات بر ارزش افزوده</returns>
    public static double CalculateTax(CalculatorVm model, double wage, double profit)
    {
        if (model.ProductType is ProductType.UsedGold or ProductType.MoltenGold)
            return 0;

        return (wage + profit) * (model.Tax / 100);
    }

    /// <summary>
    /// محاسبه قیمت نهایی.
    /// </summary>
    /// <param name="model">مدل محاسباتی</param>
    /// <param name="rawPrice">قیمت خام طلا</param>
    /// <param name="wage">اجرت ساخت</param>
    /// <param name="profit">سود فروشنده</param>
    /// <param name="tax">مالیات بر ارزش افزوده</param>
    /// <returns>قیمت نهایی</returns>
    public static double CalculateFinalPrice(CalculatorVm model, double rawPrice, double wage, double profit, double tax)
    {
        var finalPrice = rawPrice + wage + profit + tax;
        if (model.AdditionalPrices.HasValue)
        {
            finalPrice += model.AdditionalPrices.Value;
        }

        // Adjustments for Used Gold ( طلای دست دوم )
        if (model.ProductType == ProductType.UsedGold)
        {
            // In used gold, اجرت ساخت, سود فروشنده, مالیات are usually removed
            // You may need to define additional logic for كارمزد and خالصی as discussed earlier.
            finalPrice = rawPrice + (model.AdditionalPrices ?? 0); // Just the raw price + additional prices for used gold
        }

        return finalPrice;
    }

    /// <summary>
    /// دریافت نسبت عیار بر اساس نوع عیار.
    /// </summary>
    /// <param name="caratType">نوع عیار</param>
    /// <returns>نسبت عیار</returns>
    private static double GetCaratRatio(CaratType caratType)
    {
        return caratType switch
        {
            CaratType.SevenTeen => 0.708 //  17 / 24
            ,
            CaratType.Eighteen => 0.750 // 18 / 24
            ,
            CaratType.TwentyOne => 0.875 // 21 / 24
            ,
            CaratType.TwentyTwo => 0.9167 // 22 / 24
            ,
            CaratType.TwentyFour => 1.0 // 24 / 24
            ,
            _ => 0.750
        };
    }
}