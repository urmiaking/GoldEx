using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices;

public partial class Viewer
{
    [Parameter] public long Number { get; set; }

    [Parameter] public string? InvoiceType { get; set; }

    private InvoiceType _invoiceType;
    private string? _reportUrl;
    private List<BreadcrumbItem> _breadcrumbs = [];

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrEmpty(InvoiceType) && Enum.TryParse(InvoiceType, true, out _invoiceType))
        {
            _reportUrl = $"InvoiceReport?invoiceNumber={Number}&invoiceType={_invoiceType}";
        }
        else
        {
           AddErrorToast("چاپ فاکتور با خطا مواجه شد");
        }

        _breadcrumbs =
        [
            new BreadcrumbItem("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
            new BreadcrumbItem("فاکتورها", href: ClientRoutes.Invoices.Index, icon: Icons.Material.Filled.ReceiptLong),
            new BreadcrumbItem("مشاهده فاکتور", href: null, icon: Icons.Material.Filled.ViewTimeline)
        ];
    }
}