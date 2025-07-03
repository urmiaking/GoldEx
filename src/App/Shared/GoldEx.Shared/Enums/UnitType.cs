using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.Enums;

public enum UnitType
{
    [Display(Name = "ریال")]
    IRR = 0,

    [Display(Name = "دلار سنگاپور")]
    SGD = 1,

    [Display(Name = "دلار هنگ کنگ")]
    HKD = 2,

    [Display(Name = "کرون نروژ")]
    NOK = 3,

    [Display(Name = "کرون دانمارک")]
    DKK = 4,

    [Display(Name = "دلار نیوزیلند")]
    NZD = 5,

    [Display(Name = "لاری گرجستان")]
    GEL = 6,

    [Display(Name = "درام ارمنستان")]
    AMD = 7,

    [Display(Name = "منات آذربایجان")]
    AZN = 8,

    [Display(Name = "بات تایلند")]
    THB = 9,

    [Display(Name = "افغانی")]
    AFN = 10,

    [Display(Name = "دینار بحرین")]
    BHD = 11,

    [Display(Name = "کرون سوئد")]
    SEK = 12,

    [Display(Name = "لیر سوریه")]
    SYP = 13,

    [Display(Name = "فرانک سوئیس")]
    CHF = 14,

    [Display(Name = "یوآن چین")]
    CNY = 15,

    [Display(Name = "دلار کانادا")]
    CAD = 16,

    [Display(Name = "دلار استرالیا")]
    AUD = 17,

    [Display(Name = "دینار کویت")]
    KWD = 18,

    [Display(Name = "یکصد ین ژاپن")]
    JPY100 = 19,

    [Display(Name = "پوند")]
    GBP = 20,

    [Display(Name = "درهم امارات")]
    AED = 21,

    [Display(Name = "دلار")]
    USD = 22,

    [Display(Name = "روبل روسیه")]
    RUB = 23,

    [Display(Name = "رینگیت مالزی")]
    MYR = 24,

    [Display(Name = "ریال عمان")]
    OMR = 25,

    [Display(Name = "ریال قطر")]
    QAR = 26,

    [Display(Name = "دینار عراق")]
    IQD = 27,

    [Display(Name = "روپیه پاکستان")]
    PKR = 28,

    [Display(Name = "روپیه هند")]
    INR = 29,

    [Display(Name = "ریال عربستان")]
    SAR = 30,

    [Display(Name = "لیر ترکیه")]
    TRY = 31,

    [Display(Name = "یورو")]
    EUR = 32,

    [Display(Name = "طلای 18 عیار (گرم)")]
    Gold18K = 33
}