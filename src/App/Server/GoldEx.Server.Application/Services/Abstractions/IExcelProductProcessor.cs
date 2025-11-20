using GoldEx.Server.Infrastructure.Models;
using GoldEx.Server.Infrastructure.Models.Spreadsheets;
using GoldEx.Shared.DTOs.InventoryEntries;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IExcelProductProcessor
{
    Task<ProcessExcelResponse> ProcessAsync(ParseResult<ExcelProductItem> parsedData, CancellationToken cancellationToken = default);
}