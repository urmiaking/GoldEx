using GoldEx.Server.Domain.LicensePaymentAggregate;
using GoldEx.Shared.DTOs.LicensePayments;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class LicensePaymentMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LicensePayment, LicensePaymentResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);
    }
}