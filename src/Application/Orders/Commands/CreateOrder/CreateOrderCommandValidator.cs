using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Models;

namespace Order_Management.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateOrderCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .WithMessage("Customer name is required.")
            .MaximumLength(100)
            .WithMessage("Customer name must not exceed 100 characters.");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .WithMessage("Customer email is required.")
            .EmailAddress()
            .WithMessage("Customer email must be a valid email address.")
            .MaximumLength(255)
            .WithMessage("Customer email must not exceed 255 characters.");

        RuleFor(x => x.OrderItems)
            .NotEmpty()
            .WithMessage("Order must contain at least one item.")
            .Must(HasUniqueProductIds)
            .WithMessage("Order cannot contain duplicate products. Use quantity to order multiple items.");

        RuleForEach(x => x.OrderItems).SetValidator(new CreateOrderItemDtoValidator());

        // Optimized validation for products and stock
        RuleFor(x => x).CustomAsync(ValidateProductsAndStock);
    }

    public class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
    {
        public CreateOrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("Product ID must be greater than 0.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(1000)
                .WithMessage("Quantity cannot exceed 1000 items.");
        }
    }

    private static bool HasUniqueProductIds(ICollection<CreateOrderItemDto> orderItems)
    {
        var productIds = orderItems.Select(x => x.ProductId).ToList();
        return productIds.Count == productIds.Distinct().Count();
    }

    private async Task ValidateProductsAndStock(CreateOrderCommand command, ValidationContext<CreateOrderCommand> context, CancellationToken cancellationToken)
    {
        var orderItems = command.OrderItems;
        if (!orderItems.Any()) return;

        // Get unique product IDs to avoid unnecessary database queries
        var productIds = orderItems.Select(x => x.ProductId).Distinct().ToList();

        // Optimized query - only select needed fields
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.IsActive, p.StockQuantity })
            .ToListAsync(cancellationToken);

        // Create lookup dictionary for O(1) access
        var productLookup = products.ToDictionary(p => p.Id);

        // Check for missing products in batch
        var foundProductIds = products.Select(p => p.Id).ToHashSet();
        var missingProducts = productIds.Except(foundProductIds).ToList();

        foreach (var missing in missingProducts)
        {
            context.AddFailure($"Product with ID {missing} not found.");
        }

        // Group items by product ID to handle duplicate validation and sum quantities
        var itemGroups = orderItems.GroupBy(x => x.ProductId).ToList();

        foreach (var group in itemGroups)
        {
            var productId = group.Key;
            var totalQuantity = group.Sum(x => x.Quantity);

            // Skip if product doesn't exist (already handled above)
            if (!productLookup.TryGetValue(productId, out var product))
                continue;

            // Validate product is active
            if (!product.IsActive)
            {
                context.AddFailure($"Product '{product.Name} / ID {product.Id}' is not available for ordering.");
            }

            // Validate stock availability (using total quantity for this product)
            if (totalQuantity > product.StockQuantity)
            {
                context.AddFailure(
                    $"Insufficient stock for product '{product.Name} / ID {product.Id}'. Available: {product.StockQuantity}, Total requested: {totalQuantity}");
            }

            // Validate individual item quantities
            foreach (var item in group)
            {
                if (item.Quantity <= 0)
                {
                    context.AddFailure($"Invalid quantity for product '{product.Name} / ID {product.Id}'. Quantity must be greater than 0.");
                }
            }
        }
    }
}


