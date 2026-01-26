using GoldEx.Shared.Constants;

namespace GoldEx.Server.Application.Utilities;

public static class SmsParameterBuilder
{
    public static List<string> Parameters => [
        SmsTemplateParams.CustomerPhoneNumber,
        SmsTemplateParams.CustomerName,
        SmsTemplateParams.InvoiceNumber,
        SmsTemplateParams.InvoiceRemaining,
        SmsTemplateParams.InvoiceTotalAmount,
        SmsTemplateParams.InvoiceDate,
        SmsTemplateParams.InvoiceDueDate,
        SmsTemplateParams.TodayDate,
        SmsTemplateParams.InstitutionName,
        SmsTemplateParams.InstitutionPhoneNumber,
        SmsTemplateParams.InstitutionAddress
    ];

    public static string BuildForInvoice() => string.Join(",", Parameters);
}