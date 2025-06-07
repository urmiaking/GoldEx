using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Customers.ViewModels;

public class CustomerVm
{
    public Guid Id { get; set; }

    [Display(Name = "نام و نام خانوادگی")]
    [Required(ErrorMessage = "{0} الزامی است")]
    [StringLength(100, ErrorMessage = "{0} باید حداکثر {1} کاراکتر باشد")]
    public string FullName { get; set; } = default!;

    [Display(Name = "نوع مشتری")]
    public CustomerType CustomerType { get; set; } = CustomerType.Individual;

    [Display(Name = "شناسه یکتا")]
    [Required(ErrorMessage = "{0} الزامی است")]
    [StringLength(25, ErrorMessage = "{0} باید حداکثر {1} کاراکتر باشد")]
    public string NationalId { get; set; } = default!;

    [Display(Name = "شماره تماس")]
    [Required(ErrorMessage = "{0} الزامی است")]
    [StringLength(25, ErrorMessage = "{0} باید حداکثر {1} کاراکتر باشد")]
    public string PhoneNumber { get; set; } = default!;

    [Display(Name = "آدرس")]
    [StringLength(200, ErrorMessage = "{0} باید حداکثر {1} کاراکتر باشد")]
    public string? Address { get; set; }

    [Display(Name = "سقف اعتبار مشتری")]
    public decimal? CreditLimit { get; set; }

    [Display(Name = "واحد سقف اعتبار مشتری")]
    public GetPriceUnitTitleResponse? CreditLimitPriceUnit { get; set; }

    public string? CreditLimitHelperText { get; set; }
    public bool CreditLimitMenuOpen { get; set; }

    internal static CustomerVm CreateFrom(GetCustomerResponse response)
    {
        return new CustomerVm
        {
            Id = response.Id,
            FullName = response.FullName,
            CustomerType = response.CustomerType,
            NationalId = response.NationalId,
            PhoneNumber = response.PhoneNumber,
            Address = response.Address,
            CreditLimit = response.CreditLimit,
            CreditLimitPriceUnit = response.CreditLimitPriceUnit
        };
    }

    public static CustomerVm CreateDefaultInstance()
    {
        return new CustomerVm();
    }

    public static CustomerRequestDto ToCreateRequest(CustomerVm model)
    {
        return new CustomerRequestDto(null,
            model.FullName,
            model.NationalId,
            model.PhoneNumber,
            model.Address,
            model.CreditLimit,
            model.CreditLimitPriceUnit?.Id,
            model.CustomerType);
    }

    public static CustomerRequestDto ToUpdateRequest(CustomerVm model)
    {
        return new CustomerRequestDto(model.Id,
            model.FullName,
            model.NationalId,
            model.PhoneNumber,
            model.Address,
            model.CreditLimit,
            model.CreditLimitPriceUnit?.Id,
            model.CustomerType);
    }
}