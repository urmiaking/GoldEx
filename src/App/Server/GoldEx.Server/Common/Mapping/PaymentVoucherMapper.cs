using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.Enums;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class PaymentVoucherMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PaymentVoucher, GetPaymentVoucherListResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.VoucherNumber, src => src.VoucherNumber)
            .Map(dest => dest.PaymentDate, src => src.PaymentDate)
            .Map(dest => dest.Amount, src => src.Amount * (src.ExchangeRate ?? 1))
            .Map(dest => dest.PriceUnit, src => MapContext.Current.GetService<IPriceUnitRepository>()
                .Get(new PriceUnitsSetAsDefaultSpecification()).FirstOrDefault()!.Title)
            .Map(dest => dest.VoucherStatus,
                src => MapContext.Current.GetService<IInvoiceRepository>()
                    .Get(new InvoicesByVoucherIdSpecification(src.Id))
                    .Any() ? VoucherStatus.Applied : VoucherStatus.Pending)
            .Map(dest => dest.SupplierName,
                src => src.Customer != null ? src.Customer.FullName : string.Empty)
            .Map(dest => dest.SupplierPhoneNumber,
                src => src.Customer != null ? src.Customer.PhoneNumber : string.Empty)
            .Map(dest => dest.FinancialAccountType,
                src => src.SourceFinancialAccount != null
                    ? src.SourceFinancialAccount.AccountType
                    : FinancialAccountType.Cash)
            .Map(dest => dest.InvoiceId,
                src => MapContext.Current.GetService<IInvoiceRepository>()
                    .Get(new InvoicesByVoucherIdSpecification(src.Id))
                    .FirstOrDefault() != null ? MapContext.Current.GetService<IInvoiceRepository>()
                    .Get(new InvoicesByVoucherIdSpecification(src.Id))
                    .First().Id.Value : (Guid?)null);

        config.NewConfig<PaymentVoucher, GetPaymentVoucherResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.PaymentDate, src => new DateTime(src.PaymentDate.Year, src.PaymentDate.Month, src.PaymentDate.Day))
            .Map(dest => dest.VoucherNumber, src => src.VoucherNumber)
            .Map(dest => dest.Amount, src => src.Amount)
            .Map(dest => dest.PriceUnit, src => src.VoucherPriceUnit)
            .Map(dest => dest.Customer, src => src.Customer)
            .Map(dest => dest.SourceFinancialAccount, src => src.SourceFinancialAccount)
            .Map(dest => dest.DestinationFinancialAccount, src => src.DestinationFinancialAccount);
    }
}