using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IInvoiceRepository : IRepository<Invoice>,
    ICreateRepository<Invoice>,
    IUpdateRepository<Invoice>,
    IDeleteRepository<Invoice>
{
    Task<long> GetLastNumberAsync(InvoiceType invoiceType, CancellationToken cancellationToken = default);
    Task<List<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default);
    Task<List<CategorySalesRpResponse>> GetCategorySalesSummaryAsync(CategorySalesRpRequest request, CancellationToken cancellationToken = default);
    Task<List<SoldProductItemRpResponse>> GetSoldProductItemsAsync(SoldProductItemRpRequest request, CancellationToken cancellationToken = default);
    Task<List<CategorySalesComparisonRpResponse>> GetCategorySalesComparisonAsync(CategorySalesComparisonRpRequest request, CancellationToken cancellationToken = default);
}