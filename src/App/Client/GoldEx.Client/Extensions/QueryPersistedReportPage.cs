using GoldEx.Client.Components.Components;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Sdk.Common.Extensions;

namespace GoldEx.Client.Extensions;

public abstract class QueryPersistedReportPage<TFilter> : GoldExComponentBase
    where TFilter : ReportFilterVmBase, new()
{
    protected TFilter Filters { get; } = new();
    protected bool IsLoading { get; set; }

    protected abstract void ReadQueryToFilter();
    protected abstract object WriteFilterToQuery();

    protected override void OnParametersSet()
    {
        ReadQueryToFilter();
        base.OnParametersSet();
    }

    protected void PersistFiltersToQuery()
    {
        var baseUrl = Navigation.Uri.Split('?')[0];
        var url = baseUrl.AppendQueryString(WriteFilterToQuery());

        Navigation.NavigateTo(url, replace: true);
    }
}
