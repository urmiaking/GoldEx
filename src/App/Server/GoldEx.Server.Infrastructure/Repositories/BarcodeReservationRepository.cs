using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.BarcodeReservationAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class BarcodeReservationRepository(GoldExDbContext dbContext) : RepositoryBase<BarcodeReservation>(dbContext), IBarcodeReservationRepository;