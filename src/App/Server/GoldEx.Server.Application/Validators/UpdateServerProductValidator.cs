using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Application.Validators;

namespace GoldEx.Server.Application.Validators;

public class UpdateServerProductValidator : UpdateProductValidator<Product>
{
    public UpdateServerProductValidator()
    {
        Include(new CreateServerProductValidator());
    }
}