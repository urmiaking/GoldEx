using GoldEx.Shared.DTOs.Licenses;
using System;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface ILicenseCache
{
    GetLicenseResponse? Get(Guid storeId);
    void Set(Guid storeId, GetLicenseResponse license);
    void Remove(Guid storeId);
    void Clear();
}
