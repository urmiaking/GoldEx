﻿using System.ComponentModel.DataAnnotations;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Pages.Transactions.ViewModels;

public class UpdateTransactionVm
{
    public Guid Id { get; set; }

    [Display(Name = "تاریخ تراکنش")]
    [Required(ErrorMessage = "{0} الزامی است")]
    public DateTime? TransactionDate { get; set; } = DateTime.Now;

    [Display(Name = "ساعت تراکنش")]
    [Required(ErrorMessage = "{0} الزامی است")]
    public TimeSpan? TransactionTime { get; set; } = DateTime.Now.TimeOfDay;

    [Display(Name = "شماره تراکنش")]
    [Required(ErrorMessage = "{0} الزامی است")]
    public int TransactionNumber { get; set; }

    [Display(Name = "شرح تراکنش")]
    [Required(ErrorMessage = "{0} الزامی است")]
    public string Description { get; set; } = default!;

    [Display(Name = "بدهکاری")]
    public double? Debit { get; set; }

    [Display(Name = "بستانکاری")]
    public double? Credit { get; set; }

    [Display(Name = "واحد بستانکاری")]
    public UnitType? CreditUnit { get; set; }

    [Display(Name = "واحد بدهکاری")]
    public UnitType? DebitUnit { get; set; }

    [Display(Name = "نرخ تبدیل بستانکاری")]
    public double? CreditRate { get; set; }

    [Display(Name = "نرخ تبدیل بدهکاری")]
    public double? DebitRate { get; set; }

    [Display(Name = "معادل ریالی بستانکاری")]
    public double? CreditEquivalent { get; set; }

    [Display(Name = "معادل ریالی بدهکاری")]
    public double? DebitEquivalent { get; set; }

    // Customer related properties  
    public Guid? CustomerId { get; set; }

    [Display(Name = "شناسه یکتا")]
    [Required(ErrorMessage = "{0} الزامی است")]
    [StringLength(25, ErrorMessage = "{0} باید حداکثر {1} کاراکتر باشد")]
    public string CustomerNationalId { get; set; } = default!;

    [Display(Name = "نام و نام خانوادگی")]
    [Required(ErrorMessage = "{0} الزامی است")]
    [StringLength(100, ErrorMessage = "{0} باید حداکثر {1} کاراکتر باشد")]
    public string CustomerFullName { get; set; } = default!;

    [Display(Name = "شماره تماس")]
    [Required(ErrorMessage = "{0} الزامی است")]
    [StringLength(25, ErrorMessage = "{0} باید حداکثر {1} کاراکتر باشد")]
    public string CustomerPhoneNumber { get; set; } = default!;

    [Display(Name = "سقف اعتبار مشتری")]
    public double? CustomerCreditLimit { get; set; }

    [Display(Name = "واحد سقف اعتبار مشتری")]
    public UnitType? CustomerCreditLimitUnit { get; set; }

    [Display(Name = "مقدار اعتبار باقی مانده مشتری")]
    public double? CustomerCreditRemaining { get; set; }

    [Display(Name = "واحد اعتبار باقی مانده مشتری")]
    public UnitType? CustomerCreditRemainingUnit { get; set; }

    [Display(Name = "آدرس")]
    public string? CustomerAddress { get; set; }

    [Display(Name = "نوع مشتری")]
    public CustomerType CustomerType { get; set; } = CustomerType.Individual;

    public void SetTransaction(GetTransactionResponse transaction)
    {
        Id = transaction.Id;
        TransactionNumber = transaction.Number;
        Description = transaction.Description;
        TransactionDate = transaction.DateTime;
        TransactionTime = transaction.DateTime.TimeOfDay;
        Debit = transaction.Debit;
        DebitUnit = transaction.DebitUnit;
        DebitRate = transaction.DebitRate;
        Credit = transaction.Credit;
        CreditUnit = transaction.CreditUnit;
        CreditRate = transaction.CreditRate;

        SetCustomer(transaction.Customer);
    }

    public void SetCustomer(GetCustomerResponse customer)
    {
        CustomerId = customer.Id;
        CustomerType = customer.CustomerType;
        CustomerNationalId = customer.NationalId;
        CustomerFullName = customer.FullName;
        CustomerPhoneNumber = customer.PhoneNumber;
        CustomerAddress = customer.Address;
        CustomerCreditLimit = customer.CreditLimit;
        CustomerCreditLimitUnit = customer.CreditLimitUnit;
        CustomerCreditRemaining = customer.CreditLimit; // TODO: need to be calculated based on transactions
        CustomerCreditRemainingUnit = customer.CreditLimitUnit; // TODO: need to be calculated based on transactions
    }

    public static UpdateTransactionRequest ToUpdateTransactionRequest(UpdateTransactionVm model)
    {
        if (model.CustomerId is null)
            throw new ArgumentNullException(nameof(model.CustomerId));

        return new UpdateTransactionRequest(
            model.TransactionNumber,
            model.Description,
            DateTime.Today.Add(model.TransactionTime ?? TimeSpan.Zero),
            model.Credit,
            model.CreditUnit,
            model.CreditRate,
            model.Debit,
            model.DebitUnit,
            model.DebitRate,
            model.CustomerId.Value
        );
    }

    public static UpdateCustomerRequest ToUpdateCustomerRequest(UpdateTransactionVm model)
    {
        if (model.CustomerId is null)
            throw new ArgumentNullException(nameof(model.CustomerId));

        return new UpdateCustomerRequest(
            model.CustomerId.Value,
            model.CustomerFullName,
            model.CustomerNationalId,
            model.CustomerPhoneNumber,
            model.CustomerAddress,
            model.CustomerCreditLimit,
            model.CustomerCreditLimitUnit,
            model.CustomerType
        );
    }
}