using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.FinancialAccounts;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Client.Pages.Customers.ViewModels;

public class CustomerVm
{
    public Guid? Id { get; set; }

    [Display(Name = "نام و نام خانوادگی")]
    [Required(ErrorMessage = "{0} الزامی است")]
    [StringLength(100, ErrorMessage = "{0} باید حداکثر {1} کاراکتر باشد")]
    public string FullName { get; set; } = default!;

    [Display(Name = "نوع")]
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

    public bool CreditLimitMenuOpen { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<FinancialAccountVm>? FinancialAccounts { get; set; } = [];

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
            CreditLimitPriceUnit = response.CreditLimitPriceUnit,
            CreatedAt = response.CreatedAt,
            FinancialAccounts = response.FinancialAccounts?.Select(FinancialAccountVm.CreateFrom).ToList()
        };
    }

    public static CustomerRequestDto ToRequest(CustomerVm model)
    {
        List<FinancialAccountRequestDto>? bankAccounts = null;

        if (model.FinancialAccounts is not null) 
            bankAccounts = model.FinancialAccounts.Select(x => x.ToRequest()).ToList();

        return new CustomerRequestDto(model.Id,
            model.FullName,
            model.NationalId,
            model.PhoneNumber,
            model.Address,
            model.CreditLimit,
            model.CreditLimitPriceUnit?.Id,
            model.CustomerType,
            bankAccounts);
    }
}