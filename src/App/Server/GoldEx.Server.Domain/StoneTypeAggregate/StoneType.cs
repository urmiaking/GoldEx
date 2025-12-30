using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.StoneTypeAggregate;

public readonly record struct StoneTypeId(Guid Value);
public class StoneType : EntityBase<StoneTypeId>
{
    public string Title { get; private set; }
    public string EnTitle { get; private set; }
    public string Symbol { get; private set; }
    public StoneKind Kind { get; private set; }

    public bool IsActive { get; private set; }

#pragma warning disable CS8618
    private StoneType() { }
#pragma warning restore CS8618

    private StoneType(string title, string enTitle, string symbol)
    {
        Id = new StoneTypeId(Guid.NewGuid());
        Title = title;
        EnTitle = enTitle;
        Symbol = symbol;
        IsActive = true;
    }

    public static StoneType Create(string title, string enTitle, string symbol) => new(title, enTitle, symbol);

    public void Update(string title, string enTitle, string symbol)
    {
        Title = title;
        EnTitle = enTitle;
        Symbol = symbol;
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}