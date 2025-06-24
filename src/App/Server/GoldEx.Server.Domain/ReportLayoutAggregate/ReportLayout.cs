using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.ReportLayoutAggregate;

public readonly record struct ReportLayoutId(Guid Value);
public class ReportLayout : EntityBase<ReportLayoutId>
{
    public static ReportLayout Create(string name, string displayName, byte[] layoutData)
    {
        return new ReportLayout
        {
            Id = new ReportLayoutId(Guid.NewGuid()),
            Name = name,
            DisplayName = displayName,
            LayoutData = layoutData
        };
    }

    private ReportLayout() { }

    /// <summary>
    /// A unique, machine-readable name for the report (e.g., "InvoiceReport", "SalesSummary_v2").
    /// This corresponds to the 'url' in DevExpress's terminology.
    /// </summary>
    public string Name { get; private set; } = default!;

    /// <summary>
    /// A user-friendly name displayed in the "Open Report" dialog (e.g., "Customer Invoice", "Sales Summary").
    /// </summary>
    public string DisplayName { get; private set; } = default!;

    /// <summary>
    /// The raw byte data of the .repx file content (XML).
    /// </summary>
    public byte[] LayoutData { get; set; } = default!;

    public void SetName(string name) => Name = name;
    public void SetDisplayName(string displayName) => DisplayName = displayName;
    public void SetLayoutData(byte[] layoutData) => LayoutData = layoutData;
}