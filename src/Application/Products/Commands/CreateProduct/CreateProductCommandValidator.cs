using Order_Management.Application.Common.Interfaces;

namespace Order_Management.Application.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    private readonly IApplicationDbContext _context;
    public CreateProductCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(p => p!.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MinimumLength(2)
            .WithMessage("Product name must be at least 2 characters long")
            .MaximumLength(100)
            .WithMessage("Product name cannot exceed 100 characters")
            .Must(name => !string.IsNullOrWhiteSpace(name?.Trim()))
            .WithMessage("Product name cannot be empty or only whitespace")
            .Must(name => !name.StartsWith(" ") && !name.EndsWith(" "))
            .WithMessage("Product name cannot start or end with spaces")
            .Must(name => !name.Contains("  ")) // No double spaces
            .WithMessage("Product name cannot contain consecutive spaces")
            .Must(name => char.IsLetterOrDigit(name.Trim().First()))
            .WithMessage("Product name must start with a letter or number");

        RuleFor(p => p!.StockQuantity)
            .NotEmpty()
            .WithMessage("Stock quantity is required")
            .GreaterThan(0)
            .WithMessage("Cannot create a product with zero stock")
            .LessThan(1000000)
            .WithMessage("Stock quantity seems unreasonably high");

        RuleFor(p => p!.Price)
            .NotEmpty()
            .WithMessage("Price is required")
            .GreaterThan(0)
            .WithMessage("Product must have a valid price")
            .LessThan(1000000)
            .WithMessage("Price seems unreasonably high");

    }
}
