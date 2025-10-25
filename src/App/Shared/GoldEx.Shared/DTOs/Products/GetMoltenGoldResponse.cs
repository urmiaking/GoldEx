using GoldEx.Shared.DTOs.Customers;

namespace GoldEx.Shared.DTOs.Products;

public record GetMoltenGoldResponse(string? AssayNumber, GetCustomerResponse? Assayer, DateTime? AssayDate);