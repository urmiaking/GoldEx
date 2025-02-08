namespace GoldEx.Sdk.Client.Abstractions;

public class BusyIndicatorStatusChangedEventArgs(bool isBusy) : EventArgs
{
    public bool IsBusy { get; } = isBusy;
}