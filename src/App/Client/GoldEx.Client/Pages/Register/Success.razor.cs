using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Register;

public partial class Success
{
    private LicensePlan _licensePlan;

    [Parameter, SupplyParameterFromQuery, EditorRequired]
    public string PlanType { get; set; }

    protected override void OnInitialized()
    {
        _licensePlan = Enum.Parse<LicensePlan>(PlanType);

        base.OnInitialized();
    }

    private void GoToHomePage()
    {
        Navigation.NavigateTo(ClientRoutes.Home.Index, forceLoad: true);
    }
}