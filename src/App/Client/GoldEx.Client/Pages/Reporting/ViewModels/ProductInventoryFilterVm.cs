using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Client.Pages.Reporting.ViewModels;

public class ProductInventoryFilterVm : ReportFilterVmBase
{
    [Display(Name = "وضعیت کالا")]
    public ItemStatus? ItemStatus { get; set; }

    [Display(Name = "نوع کالا")]
    public ItemType? ItemType { get; set; }

    [Display(Name = "دسته بندی کالا")]
    public Guid? ProductCategoryId { get; set; }

    public ProductInventoryRpRequest ToRequest()
    {
        return new ProductInventoryRpRequest(ItemStatus,
            ItemType,
            ProductCategoryId,
            DateRange?.Start?.GetDayStart(),
            DateRange?.End?.GetDayEnd());
    }

    public class ProductInventorySummary
    {
        public decimal TotalSoldAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalRemainingAmount => TotalAmount - TotalSoldAmount;
    }
}