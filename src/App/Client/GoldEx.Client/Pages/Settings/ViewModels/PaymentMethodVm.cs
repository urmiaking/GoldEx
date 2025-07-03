using GoldEx.Shared.DTOs.PaymentMethods;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class PaymentMethodVm
{
    public Guid Id { get; set; }

    public int Index { get; set; }

    [Display(Name = "عنوان")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string Title { get; set; } = default!;

    [Display(Name = "وضعیت")]
    public bool IsActive { get; set; }

    public static PaymentMethodVm CreateFrom(GetPaymentMethodResponse response)
    {
        return new PaymentMethodVm
        {
            Id = response.Id,
            Title = response.Title,
            IsActive = response.IsActive
        };
    }

    public static CreatePaymentMethodRequest ToCreateRequest(PaymentMethodVm item)
    {
        return new CreatePaymentMethodRequest(item.Title);
    }

    public static UpdatePaymentMethodRequest ToUpdateRequest(PaymentMethodVm item)
    {
        return new UpdatePaymentMethodRequest(item.Title);
    }
}