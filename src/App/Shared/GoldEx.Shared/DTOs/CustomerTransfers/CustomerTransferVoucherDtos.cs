using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
using System.ComponentModel.DataAnnotations;

namespace GoldEx.Shared.DTOs.CustomerTransfers;

public class CreateCustomerTransferVoucherRequest
{
    [Required(ErrorMessage = "تاریخ حواله الزامی است.")]
    public DateOnly TransferDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    [Required(ErrorMessage = "مشتری حواله‌دهنده الزامی است.")]
    public Guid SourceCustomerId { get; set; }

    [Required(ErrorMessage = "مشتری دریافت‌کننده الزامی است.")]
    public Guid DestinationCustomerId { get; set; }

    [Required(ErrorMessage = "واحد قیمت الزامی است.")]
    public Guid PriceUnitId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "مبلغ یا وزن حواله باید بزرگتر از صفر باشد.")]
    public decimal Amount { get; set; }

    public decimal? ExchangeRate { get; set; }

    public Guid? SourceInvoiceId { get; set; }

    public Guid? DestinationInvoiceId { get; set; }

    public string? Description { get; set; }
}

public class UpdateCustomerTransferVoucherRequest : CreateCustomerTransferVoucherRequest
{
    public Guid Id { get; set; }
}

public class GetCustomerTransferVoucherResponse
{
    public Guid Id { get; set; }
    public long VoucherNumber { get; set; }
    public DateOnly TransferDate { get; set; }

    public Guid SourceCustomerId { get; set; }
    public string SourceCustomerName { get; set; } = string.Empty;

    public Guid DestinationCustomerId { get; set; }
    public string DestinationCustomerName { get; set; } = string.Empty;

    public Guid PriceUnitId { get; set; }
    public string PriceUnitTitle { get; set; } = string.Empty;
    public bool PriceUnitIsGoldBased { get; set; }

    public decimal Amount { get; set; }
    public decimal? ExchangeRate { get; set; }

    public Guid? SourceInvoiceId { get; set; }
    public long? SourceInvoiceNumber { get; set; }

    public Guid? DestinationInvoiceId { get; set; }
    public long? DestinationInvoiceNumber { get; set; }

    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetCustomerTransferVoucherListResponse
{
    public Guid Id { get; set; }
    public long VoucherNumber { get; set; }
    public DateOnly TransferDate { get; set; }

    public Guid SourceCustomerId { get; set; }
    public string SourceCustomerName { get; set; } = string.Empty;

    public Guid DestinationCustomerId { get; set; }
    public string DestinationCustomerName { get; set; } = string.Empty;

    public Guid PriceUnitId { get; set; }
    public string PriceUnitTitle { get; set; } = string.Empty;
    public bool PriceUnitIsGoldBased { get; set; }

    public decimal Amount { get; set; }
    public decimal? ExchangeRate { get; set; }

    public Guid? SourceInvoiceId { get; set; }
    public long? SourceInvoiceNumber { get; set; }

    public Guid? DestinationInvoiceId { get; set; }
    public long? DestinationInvoiceNumber { get; set; }

    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CustomerTransferVoucherFilter
{
    public Guid? SourceCustomerId { get; set; }
    public Guid? DestinationCustomerId { get; set; }
    public Guid? PriceUnitId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? SearchTerm { get; set; }
}
