using GoldEx.Server.Domain.CustomerTransferVoucherAggregate;
using GoldEx.Shared.DTOs.CustomerTransfers;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerCustomerTransferVoucherService
{
    Task<CustomerTransferVoucher> PostAsync(CreateCustomerTransferVoucherRequest request, CancellationToken cancellationToken,
        bool isMoltenGoldDesk = false);
}
