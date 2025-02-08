namespace GoldEx.Sdk.Client.Abstractions;

public interface IBusyIndicator
{
    event EventHandler<BusyIndicatorStatusChangedEventArgs>? StatusChanged;

    void SetBusy();
    void SetIdeal();
}