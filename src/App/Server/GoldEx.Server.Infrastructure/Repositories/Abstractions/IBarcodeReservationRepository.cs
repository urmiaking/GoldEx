using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.BarcodeReservationAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IBarcodeReservationRepository : IRepository<BarcodeReservation>,
    ICreateRepository<BarcodeReservation>,
    IUpdateRepository<BarcodeReservation>,
    IDeleteRepository<BarcodeReservation>;