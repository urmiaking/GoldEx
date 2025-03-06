using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Validators;

public class UpdateProductValidator<T> : AbstractValidator<T> where T : ProductBase
{
    public UpdateProductValidator(IProductRepository<T> repository)
    {
        Include(new CreateProductValidator<T>(repository));

        RuleFor(x => x.Id)
            .NotEqual(new ProductId(Guid.Empty)).WithMessage("شناسه کالا نمی تواند خالی باشد");
    }
}