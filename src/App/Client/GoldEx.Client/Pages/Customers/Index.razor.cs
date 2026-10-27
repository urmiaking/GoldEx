using Microsoft.AspNetCore.Components;
using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers;

public partial class Index
{
    [Parameter, SupplyParameterFromQuery(Name = "create")] public bool CreateRequested { get; set; }
    private Components.CustomersList? _list;
    private bool _pendingCreate;
    private bool _consumingCreate;
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (CreateRequested && !_consumingCreate)
        {
            _pendingCreate = true;
            _consumingCreate = true;
            // Finish the URL change first: MudDialogProvider dismisses dialogs on navigation.
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("create", (bool?)null), replace: true);
        }
        else if (!CreateRequested && _pendingCreate && _list is not null)
        {
            _pendingCreate = false;
            _consumingCreate = false;
            await _list.OnCreate();
        }
        await base.OnAfterRenderAsync(firstRender);
    }
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("لیست مشتریان", href: ClientRoutes.Customers.Index, icon: Icons.Material.Filled.PeopleAlt)
    ];
}