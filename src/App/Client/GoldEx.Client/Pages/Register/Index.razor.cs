using GoldEx.Shared.Routings;

namespace GoldEx.Client.Pages.Register;

public partial class Index
{
    private void GoToRegisterForm()
    {
        Navigation.NavigateTo(ClientRoutes.RegisterProduct.Form);
    }
}