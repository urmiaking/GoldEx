using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Helpers;

public class CalculatorHelper
{
    public static class Product
    {
        /// <summary>
        /// محاسبه قیمت خام طلا بر اساس وزن، عیار و قیمت گرم طلا.
        /// </summary>
        /// <param name="weight">وزن بر حسب گرم</param>
        /// <param name="gramPrice">نرخ گرم بر اساس ریال</param>
        /// <param name="fineness">عیار</param>
        /// <param name="quantity">تعداد</param>
        /// <param name="productType">نوع محصول</param>
        /// <returns>قیمت خام طلا</returns>
        public static decimal CalculateRawPrice(decimal weight, decimal gramPrice, decimal fineness, int quantity, ProductType productType)
        {
            if (productType == ProductType.UsedGold)
            {
                return weight * fineness / 750m * gramPrice;
            }

            var gramPrice24 = gramPrice / 0.75m;
            var caratRatio = GetFinenessRatio(fineness);
            return weight * caratRatio * gramPrice24 * quantity;
        }

        /// <summary>
        /// محاسبه اجرت ساخت بر اساس قیمت خام، اجرت، نوع اجرت و نرخ تبدیل
        /// </summary>
        /// <param name="rawPrice">قیمت خام طلا</param>
        /// <param name="weight">وزن</param>
        /// <param name="wageAmount">اجرت</param>
        /// <param name="wageType">نوع اجرت</param>
        /// <param name="exchangeRate">نرخ تبدیل دلار</param>
        /// <returns>اجرت ساخت</returns>
        public static decimal CalculateWage(decimal rawPrice, decimal weight, decimal? wageAmount, WageType? wageType, decimal? exchangeRate)
        {
            if (wageAmount == null) return 0;

            decimal wage;

            switch (wageType)
            {
                case WageType.Percent:
                    wage = rawPrice * (wageAmount.Value / 100);
                    break;
                case WageType.Fixed:
                    if (exchangeRate == null)
                        wage = wageAmount.Value * weight;
                    else
                        wage = wageAmount.Value * exchangeRate.Value * weight;
                    break;
                default:
                    return 0;
            }

            return wage;
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
            if (productType is ProductType.UsedGold)
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
        /// <param name="stoneAmount">ارزش سنگ</param>
        /// <returns>مالیات بر ارزش افزوده</returns>
        public static decimal CalculateTax(decimal wage, decimal profit, decimal taxPercent, ProductType productType, decimal? stoneAmount)
        {
            if (productType is ProductType.UsedGold or ProductType.MoltenGold)
                return 0;

            return (wage + profit + (stoneAmount ?? 0)) * (taxPercent / 100);
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
            if (productType == ProductType.UsedGold)
            {
                return rawPrice + (additionalPrices ?? 0);
            }

            var result = rawPrice + wage + profit + tax + (additionalPrices ?? 0);

            return result;
        }

        /// <summary>
        /// دریافت نسبت عیار بر اساس نوع عیار.
        /// </summary>
        /// <param name="fineness">نوع عیار</param>
        /// <returns>نسبت عیار</returns>
        private static decimal GetFinenessRatio(decimal fineness) => fineness / 1000m;
    }

    public static class Coin
    {
        /// <summary>
        /// محاسبه سود هر واحد سکه
        /// </summary>
        /// <param name="unitPrice">قیمت واحد</param>
        /// <param name="profitPercent">درصد سود</param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static decimal CalculateProfit(decimal unitPrice, decimal profitPercent, int quantity)
        {
            return unitPrice * (profitPercent / 100m) * quantity;
        }
    }

    public static class Currency
    {
        public static decimal CalculateProfit(decimal unitPrice, decimal amount, decimal profitPercent)
        {
            return unitPrice * amount * (profitPercent / 100m);
        }

        public static decimal CalculateTax(decimal unitPrice, decimal amount, decimal taxPercent)
        {
            return unitPrice * amount * (taxPercent / 100m);
        }
    }

    public static class UsedProduct
    {
        /// <summary>
        /// محاسبه قیمت طلای کهنه با درنظر گرفتن وزن معادل 750 و کسری عیار از 750.
        /// </summary>
        /// <param name="weight">وزن واقعی طلا (گرم)</param>
        /// <param name="fineness">عیار واقعی طلا (مثلاً 750 یا 867)</param>
        /// <param name="deductionFrom750">مقدار کسری عیار از 750 (مثلاً 15 یعنی 750→735)</param>
        /// <param name="gramPrice">نرخ روز هر گرم طلای 750</param>
        /// <param name="quantity">تعداد اقلام</param>
        /// <param name="exchangeRate">نرخ ارز در صورت نیاز</param>
        /// <returns>قیمت نهایی خرید طلای کهنه</returns>
        public static decimal Calculate(
            decimal weight,
            decimal fineness,
            decimal deductionFrom750,
            decimal gramPrice,
            int quantity = 1,
            decimal? exchangeRate = null)
        {
            if (weight <= 0)
                throw new ArgumentOutOfRangeException(nameof(weight));
            if (gramPrice <= 0)
                throw new ArgumentOutOfRangeException(nameof(gramPrice));
            if (fineness <= 0)
                throw new ArgumentOutOfRangeException(nameof(fineness));

            var rate = exchangeRate ?? 1;

            // 1. وزن معادل بر اساس عیار واقعی
            var equivalentWeight = weight * (fineness / 750m);

            // 2. اعمال کسری عیار از 750
            var effectiveWeight = equivalentWeight * ((750m - deductionFrom750) / 750m);

            // 3. محاسبه قیمت نهایی
            var price = effectiveWeight * gramPrice * rate * quantity;

            return decimal.Round(price, 0, MidpointRounding.AwayFromZero);
        }
    }

    public static class MoltenGold
    {
        public static decimal Calculate(decimal weight, decimal fineness, decimal gramPrice, decimal? exchangeRate)
        {
            if (fineness <= 0 || gramPrice <= 0 || weight <= 0)
                throw new ArgumentOutOfRangeException(
                    $"{nameof(weight)}, {nameof(fineness)}, and {nameof(gramPrice)} must be greater than zero.");

            return weight * (fineness / 750m) * (gramPrice * (exchangeRate ?? 1));
        }
    }
}