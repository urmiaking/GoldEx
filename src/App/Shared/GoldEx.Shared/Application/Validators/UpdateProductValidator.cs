using FluentValidation;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;

namespace GoldEx.Shared.Application.Validators;

public class UpdateProductValidator<T> : AbstractValidator<T> where T : ProductBase
{
    public UpdateProductValidator()
    {
        Include(new CreateProductValidator<T>());

        RuleFor(x => x.Id)
            .NotEqual(new ProductId(Guid.Empty)).WithMessage("شناسه کالا نمی تواند خالی باشد");
    }
}