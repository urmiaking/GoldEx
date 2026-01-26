using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.SmsTemplates;

namespace GoldEx.Client.Pages.Settings.ViewModels;

public class SmsTemplateVm
{
    public List<SmsTemplateItemVm> Items { get; set; } = [];

    public static SmsTemplateVm CreateFrom(List<SmsTemplateResponse> responses)
    {
        return new SmsTemplateVm
        {
            Items = responses.Select(SmsTemplateItemVm.CreateFrom).ToList()
        };
    }

    public List<SmsTemplateRequest> ToRequests() => Items.Select(itemVm => itemVm.ToRequest()).ToList();
}

public class SmsTemplateItemVm
{
    public Guid Id { get; set; }

    [Display(Name = "متن پیامک")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public string? Body { get; set; }

    [Display(Name = "وضعیت")]
    public bool IsActive { get; set; }

    public static SmsTemplateItemVm CreateFrom(SmsTemplateResponse response)
    {
        return new SmsTemplateItemVm
        {
            Id = response.Id,
            Body = response.Body,
            IsActive = response.IsActive
        };
    }

    public SmsTemplateRequest ToRequest() => new(Id, Body ?? string.Empty, IsActive);
}