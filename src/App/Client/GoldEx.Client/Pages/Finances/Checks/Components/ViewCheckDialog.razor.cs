using GoldEx.Shared.DTOs.CheckPayments;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.Checks.Components;

public partial class ViewCheckDialog
{
    [Parameter, EditorRequired] public GetCheckPaymentListResponse Check { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance Dialog { get; set; } = default!;

    private void Close() => Dialog.Close();

    private Color GetStatusColor(CheckPaymentStatus status) => status switch
    {
        CheckPaymentStatus.Accepted => Color.Success,
        CheckPaymentStatus.Pending => Color.Primary,
        CheckPaymentStatus.Returned => Color.Error,
        _ => Color.Default
    };

    private Color GetDescriptionColor() => Check.CurrentStatus switch
    {
        CheckPaymentStatus.Accepted => Color.Success,
        CheckPaymentStatus.Returned => Color.Error,
        _ => Color.Primary
    };

    private string GetDescriptionBorderClass() => Check.CurrentStatus switch
    {
        CheckPaymentStatus.Accepted => "mud-border-success",
        CheckPaymentStatus.Returned => "mud-border-error",
        _ => "mud-border-primary"
    };

    private string GetDescriptionBgColor() => Check.CurrentStatus switch
    {
        CheckPaymentStatus.Accepted => "rgba(76, 175, 80, 0.03)",
        CheckPaymentStatus.Returned => "rgba(244, 67, 54, 0.03)",
        _ => "var(--mud-palette-background-grey)"
    };

    private string GetDescriptionIcon() => Check.CurrentStatus switch
    {
        CheckPaymentStatus.Returned => Icons.Material.Filled.AssignmentReturn,
        CheckPaymentStatus.Accepted => Icons.Material.Filled.CheckCircle,
        _ => Icons.Material.Filled.Comment
    };

    private string GetDescriptionTitle() => Check.CurrentStatus switch
    {
        CheckPaymentStatus.Returned => "دلیل برگشت چک",
        CheckPaymentStatus.Accepted => "توضیحات وصول چک",
        _ => "توضیحات آخرین تغییرات"
    };
}
