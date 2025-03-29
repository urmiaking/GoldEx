using GoldEx.Client.Abstractions.Common;
using GoldEx.Client.Abstractions.HttpServices;
using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Abstractions.SyncServices;
using GoldEx.Client.Offline.Domain.CustomerAggregate;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Customers;

namespace GoldEx.Client.Services.SyncServices;

[ScopedService]
public class CustomerSyncService(
    INetworkStatusService networkStatusService,
    ICustomerLocalClientService customerLocalService,
    ICustomerHttpClientService customerHttpService,
    ICheckpointLocalClientService checkpointService
    ) : ICustomerSyncService
{
    public async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        var isOnline = await networkStatusService.IsOnlineAsync(cancellationToken);

        if (isOnline)
        {
            var checkPoint = await checkpointService.GetLastCheckPointAsync(nameof(Customer), cancellationToken);

            var lastCheckDate = checkPoint?.SyncDate ?? DateTime.MinValue;

            await SyncFromServerAsync(lastCheckDate, cancellationToken);
            await SyncToServerAsync(lastCheckDate, cancellationToken);
        }
    }

    private async Task SyncToServerAsync(DateTime checkPointDate, CancellationToken cancellationToken = default)
    {
        var pendingCustomers = await customerLocalService.GetPendingsAsync(checkPointDate, cancellationToken);

        var shouldAddCheckpoint = true;

        foreach (var customer in pendingCustomers)
        {
            switch (customer.Status)
            {
                case ModifyStatus.Created:
                    {
                        var request = new CreateCustomerRequest(customer.Id,
                            customer.FullName,
                            customer.NationalId,
                            customer.PhoneNumber,
                            customer.Address,
                            customer.CreditLimit,
                            customer.CreditLimitUnit,
                            customer.CustomerType);

                        var created = await customerHttpService.CreateAsync(request, cancellationToken);
                        if (created)
                            await customerLocalService.SetSyncedAsync(customer.Id, cancellationToken);
                        else
                            shouldAddCheckpoint = false;

                        break;
                    }
                case ModifyStatus.Updated:
                    {
                        var request = new UpdateCustomerRequest(customer.FullName,
                            customer.NationalId,
                            customer.PhoneNumber,
                            customer.Address,
                            customer.CreditLimit,
                            customer.CreditLimitUnit,
                            customer.CustomerType);

                        var updated = await customerHttpService.UpdateAsync(customer.Id, request, cancellationToken);
                        if (updated)
                            await customerLocalService.SetSyncedAsync(customer.Id, cancellationToken);
                        else
                            shouldAddCheckpoint = false;

                        break;
                    }
                case ModifyStatus.Deleted:
                    var deleted = await customerHttpService.DeleteAsync(customer.Id, false, cancellationToken);
                    if (deleted)
                        await customerLocalService.DeleteAsync(customer.Id, true, cancellationToken);
                    else
                        shouldAddCheckpoint = false;

                    break;
                case ModifyStatus.Synced:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (shouldAddCheckpoint)
            await checkpointService.AddCheckPointAsync(nameof(Customer), cancellationToken);
    }

    private async Task SyncFromServerAsync(DateTime checkPointDate, CancellationToken cancellationToken = default)
    {
        var incomingCustomers = await customerHttpService.GetPendingsAsync(checkPointDate, cancellationToken);

        foreach (var incomingCustomer in incomingCustomers)
        {
            if (incomingCustomer.IsDeleted is true)
            {
                await customerLocalService.DeleteAsync(incomingCustomer.Id, true, cancellationToken);
            }
            else
            {
                var localCustomer = await customerLocalService.GetAsync(incomingCustomer.Id, cancellationToken);

                // Incoming customer is not available on client so create it
                if (localCustomer is null)
                {
                    var createRequest = new CreateCustomerRequest(incomingCustomer.Id,
                        incomingCustomer.FullName,
                        incomingCustomer.NationalId,
                        incomingCustomer.PhoneNumber,
                        incomingCustomer.Address,
                        incomingCustomer.CreditLimit,
                        incomingCustomer.CreditLimitUnit,
                        incomingCustomer.CustomerType);

                    await customerLocalService.CreateAsSyncedAsync(createRequest, cancellationToken);
                }

                // Incoming customer is available on client so update it
                else
                {
                    var updateRequest = new UpdateCustomerRequest(incomingCustomer.FullName,
                        incomingCustomer.NationalId,
                        incomingCustomer.PhoneNumber,
                        incomingCustomer.Address,
                        incomingCustomer.CreditLimit,
                        incomingCustomer.CreditLimitUnit,
                        incomingCustomer.CustomerType);

                    await customerLocalService.UpdateAsSyncAsync(incomingCustomer.Id, updateRequest, cancellationToken);
                }
            }
        }
    }

}