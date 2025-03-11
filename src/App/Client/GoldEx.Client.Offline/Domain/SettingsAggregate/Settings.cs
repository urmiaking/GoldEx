using GoldEx.Shared.Domain.Aggregates.SettingsAggregate;

namespace GoldEx.Client.Offline.Domain.SettingsAggregate;

public class Settings : SettingsBase
{
    public Settings(string institutionName,
        string address,
        string phoneNumber,
        double tax,
        double goldProfit,
        double jewelryProfit) : base(institutionName,
        address,
        phoneNumber,
        tax,
        goldProfit,
        jewelryProfit)
    {
    }

    public Settings(SettingsId id,
        string institutionName,
        string address,
        string phoneNumber,
        double tax,
        double goldProfit,
        double jewelryProfit) : base(id,
        institutionName,
        address,
        phoneNumber,
        tax,
        goldProfit,
        jewelryProfit)
    {
    }
}