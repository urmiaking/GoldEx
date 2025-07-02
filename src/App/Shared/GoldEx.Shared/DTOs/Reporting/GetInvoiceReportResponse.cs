using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Settings;

namespace GoldEx.Shared.DTOs.Reporting;

public record GetInvoiceReportResponse(GetInvoiceDetailResponse Invoice, GetSettingResponse Setting);