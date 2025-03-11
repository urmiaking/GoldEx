using GoldEx.Shared.Domain.Aggregates.SettingsAggregate;

namespace GoldEx.Server.Domain.SettingsAggregate;

public class Settings : SettingsBase
{
    public Settings(string institutionName,
        string address,
        string phoneNumber,
        double tax,
        double profit) : base(institutionName,
        address,
        phoneNumber,
        tax,
        profit)
    {
    }

    public Settings(SettingsId id,
        string institutionName,
        string address,
        string phoneNumber,
        double tax,
        double profit) : base(id,
        institutionName,
        address,
        phoneNumber,
        tax,
        profit)
    {
    }
}