namespace GoldEx.Shared.DTOs.Reporting;

/// <summary>A read-only customer dossier; invoices/payments are descriptive and never posted again.</summary>
public sealed record CustomerComprehensiveRpRequest(
    Guid CustomerId,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    bool IncludeDetails = true);
