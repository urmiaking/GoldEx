using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Application.Validators.Products;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Application.Validators;

public class UpdateServerProductValidator : UpdateProductValidator<Product>
{
    public UpdateServerProductValidator(IProductRepository<Product> repository) : base(repository)
    {
        Include(new CreateServerProductValidator(repository));
    }
}