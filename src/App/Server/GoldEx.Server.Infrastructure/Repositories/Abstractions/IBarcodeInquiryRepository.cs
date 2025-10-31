using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.BarcodeInquiryAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IBarcodeInquiryRepository : IRepository<BarcodeInquiry>,
    ICreateRepository<BarcodeInquiry>,
    IUpdateRepository<BarcodeInquiry>,
    IDeleteRepository<BarcodeInquiry>;