using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.MeltingBatchAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IMeltingBatchRepository : IRepository<MeltingBatch>,
    ICreateRepository<MeltingBatch>,
    IUpdateRepository<MeltingBatch>,
    IDeleteRepository<MeltingBatch>;