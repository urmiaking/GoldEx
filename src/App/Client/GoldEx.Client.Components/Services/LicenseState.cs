using GoldEx.Shared.DTOs.Licenses;

namespace GoldEx.Client.Components.Services;

public sealed class LicenseState
{
    public GetLicenseResponse? Current { get; private set; }

    public event Action? OnChange;

    public void Set(GetLicenseResponse? license)
    {
        Current = license;
        Notify();
    }

    public void Clear()
    {
        Current = null;
        Notify();
    }

    private void Notify() => OnChange?.Invoke();
}