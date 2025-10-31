using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.BarcodeInquiryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class BarcodeInquiryRepository(GoldExDbContext dbContext) : RepositoryBase<BarcodeInquiry>(dbContext), IBarcodeInquiryRepository;