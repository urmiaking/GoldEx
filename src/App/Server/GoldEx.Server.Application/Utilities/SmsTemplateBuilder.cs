namespace GoldEx.Server.Application.Utilities;

public static class SmsTemplateBuilder
{
    public static string BuildForInvoice()
    {
        var parameters = SmsParameterBuilder.Parameters;

        var message = $"""
                       ({parameters[1]}) عزیز
                       فاکتور شماره ({parameters[2]}) شما به مبلغ ({parameters[4]}) صادر شده است.
                       مبلغ پرداخت نشده این فاکتور ({parameters[3]}) می‌باشد.
                       لطفا در اسرع وقت نسبت به تسویه آن اقدام فرمایید.
                       
                       با تشکر، ({parameters[7]})
                       """;

        return message;
    }
}