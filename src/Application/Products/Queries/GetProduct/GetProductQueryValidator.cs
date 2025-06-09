using Order_Management.Application.Common.Interfaces;

namespace Order_Management.Application.Products.Queries.GetProduct;

public class GetProductQueryValidator : AbstractValidator<GetProductQuery>
{
    private readonly IApplicationDbContext _context;
    public GetProductQueryValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required")
            .GreaterThan(0)
            .WithMessage("Product ID must be a positive number");
    }
}
