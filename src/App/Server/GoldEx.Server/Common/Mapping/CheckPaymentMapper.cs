using System.Linq;
using GoldEx.Server.Domain.CheckPaymentAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Shared.DTOs.CheckPayments;
using GoldEx.Shared.Enums;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class CheckPaymentMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CheckPayment, GetCheckPaymentListResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.InvoicePaymentId, src => src.InvoicePaymentId.Value)
            .Map(dest => dest.Number, src => src.Number)
            .Map(dest => dest.SayadiCode, src => src.SayadiCode)
            .Map(dest => dest.DueDate, src => src.DueDate)
            .Map(dest => dest.CurrentStatus, src => src.CurrentStatus)
            .Map(dest => dest.LastModifiedAt, src => src.LastModifiedAt)
            .Map(dest => dest.IssuerId, src => src.IssuerId.Value)
            .Map(dest => dest.IssuerFullName, src => src.Issuer != null ? src.Issuer.FullName : string.Empty)
            .Map(dest => dest.IssuerPhoneNumber, src => src.Issuer != null ? src.Issuer.PhoneNumber : null)
            .Map(dest => dest.IssuerFinancialAccountId, src => src.IssuerFinancialAccountId.Value)
            .Map(dest => dest.IssuerFinancialAccountName, src => src.IssuerFinancialAccount != null ? src.IssuerFinancialAccount.GetAccountTypeText() : string.Empty)
            .Map(dest => dest.Amount, src => src.InvoicePayment != null ? src.InvoicePayment.Amount : 0)
            .Map(dest => dest.PriceUnit, src => (src.InvoicePayment != null && src.InvoicePayment.PriceUnit != null) ? src.InvoicePayment.PriceUnit.Title : string.Empty)
            .Map(dest => dest.PriceUnitId, src => src.InvoicePayment != null ? src.InvoicePayment.PriceUnitId.Value : Guid.Empty)
            .Map(dest => dest.InvoiceNumber, src => (src.InvoicePayment != null && src.InvoicePayment.Invoice != null) ? src.InvoicePayment.Invoice.InvoiceNumber : 0)
            .Map(dest => dest.InvoiceType, src => (src.InvoicePayment != null && src.InvoicePayment.Invoice != null) ? src.InvoicePayment.Invoice.InvoiceType : default)
            .Map(dest => dest.ImageUrl, src => (string?)null)
            .Map(dest => dest.TargetFinancialAccountName, src =>
                src.ChangeLogs
                    .OrderByDescending(cl => cl.DateTime)
                    .FirstOrDefault(cl => cl.Status == CheckPaymentStatus.Accepted && cl.TargetFinancialAccount != null)
                    != null
                        ? src.ChangeLogs
                            .OrderByDescending(cl => cl.DateTime)
                            .First(cl => cl.Status == CheckPaymentStatus.Accepted && cl.TargetFinancialAccount != null)
                            .TargetFinancialAccount!.GetAccountTypeText()
                        : null)
            .Map(dest => dest.Description, src =>
                src.ChangeLogs
                    .OrderByDescending(cl => cl.DateTime)
                    .FirstOrDefault() != null
                        ? src.ChangeLogs
                            .OrderByDescending(cl => cl.DateTime)
                            .First().Description
                        : null);
    }
}
