using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.InventoryExits;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Results;
using ValidationException = FluentValidation.ValidationException;

namespace GoldEx.Client.Pages.InventoryExits.ViewModels;

public class InventoryExitVm
{
    [Display(Name = "دلیل خروج")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public ExitReason? ExitReason { get; set; }

    [Display(Name = "تاریخ خروج")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public DateTime? ExitDate { get; set; }

    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    public List<ProductItemVm> ProductItems { get; set; } = [];
    public List<CoinItemVm> CoinItems { get; set; } = [];
    public List<CurrencyItemVm> CurrencyItems { get; set; } = [];

    public CreateInventoryExitRequest ToRequest()
    {
        if (!ProductItems.Any() && !CoinItems.Any() & !CurrencyItems.Any())
            throw new ValidationException("حداقل یک آیتم برای خروج باید وارد شود");

        List<CreateProductItemExitRequest> productItems = [];
        List<CreateCoinItemExitRequest> coinItems = [];
        List<CreateCurrencyItemExitRequest> currencyItems = [];

        productItems.AddRange(ProductItems.Select(ProductItemVm.ToInventoryExitRequest));
        coinItems.AddRange(CoinItems.Select(CoinItemVm.ToInventoryExitRequest));
        currencyItems.AddRange(CurrencyItems.Select(CurrencyItemVm.ToInventoryExitRequest));

        return new CreateInventoryExitRequest(ExitReason ?? throw new ArgumentNullException(),
            ExitDate ?? throw new ArgumentNullException(),
            Description,
            productItems,
            coinItems,
            currencyItems);
    }
}