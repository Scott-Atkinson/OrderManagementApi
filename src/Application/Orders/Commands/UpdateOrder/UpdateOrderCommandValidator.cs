using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Models;
using Order_Management.Domain.Enums;

namespace Order_Management.Application.Orders.Commands.UpdateOrder;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateOrderCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.OrderId)
            .GreaterThan(0)
            .WithMessage("Order ID must be greater than 0.");

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
            .Must(HasValidItems)
            .WithMessage("Order must contain at least one valid item with quantity greater than 0.");

        RuleForEach(x => x.OrderItems).SetValidator(new UpdateOrderItemDtoValidator());

        // Single comprehensive validation using CustomAsync and AddFailure
        RuleFor(x => x).CustomAsync(ValidateOrderAndItems);
    }

    public class UpdateOrderItemDtoValidator : AbstractValidator<UpdateOrderItemDto>
    {
        public UpdateOrderItemDtoValidator()
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

    private static bool HasValidItems(ICollection<UpdateOrderItemDto> orderItems)
    {
        return orderItems.Any(x => x.Quantity > 0);
    }

    private async Task ValidateOrderAndItems(UpdateOrderCommand command, ValidationContext<UpdateOrderCommand> context, CancellationToken cancellationToken)
    {
        // Get all required data in a single query batch
        var productIds = command.OrderItems.Select(x => x.ProductId).Distinct().ToList();
        var existingItemIds = command.OrderItems.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();

        // Execute all database queries in parallel
        var orderTask = _context.Orders
            .AsNoTracking()
            .Where(o => o.Id == command.OrderId)
            .Select(o => new { o.Id, o.OrderNumber, o.Status })
            .FirstOrDefaultAsync(cancellationToken);

        var productsTask = _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.IsActive })
            .ToListAsync(cancellationToken);

        var validItemIdsTask = existingItemIds.Any()
            ? _context.OrderItems
                .AsNoTracking()
                .Where(oi => existingItemIds.Contains(oi.Id) && oi.OrderId == command.OrderId)
                .Select(oi => oi.Id)
                .ToListAsync(cancellationToken)
            : Task.FromResult(new List<int>());

        // Wait for all queries to complete
        await Task.WhenAll(orderTask, productsTask, validItemIdsTask);

        var order = await orderTask;
        var products = await productsTask;
        var validItemIds = await validItemIdsTask;

        // Validate order existence and status using AddFailure
        if (order == null)
        {
            context.AddFailure(nameof(command.OrderId), $"Order with ID {command.OrderId} not found.");
            return; // Early exit if order doesn't exist
        }

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
        {
            context.AddFailure(nameof(command.OrderId), $"Cannot update order {order.OrderNumber}. Only pending and processing orders can be updated. Current status: {order.Status}");
        }

        // Create lookup dictionaries for efficient validation
        var productLookup = products.ToDictionary(p => p.Id);
        var validItemIdSet = validItemIds.ToHashSet();

        // Validate products exist
        var missingProducts = productIds.Except(products.Select(p => p.Id)).ToList();
        foreach (var missingProduct in missingProducts)
        {
            context.AddFailure($"Product with ID {missingProduct} not found.");
        }

        // Validate order items
        foreach (var item in command.OrderItems)
        {
            // Validate product exists and is active
            if (productLookup.TryGetValue(item.ProductId, out var product))
            {
                if (!product.IsActive)
                {
                    context.AddFailure($"Product '{product.Name} / ID {product.Id}' is not available for ordering.");
                }
            }

            // Validate existing item IDs belong to this order
            if (item.Id.HasValue && !validItemIdSet.Contains(item.Id.Value))
            {
                context.AddFailure($"Order item ID {item.Id.Value} not found or does not belong to this order.");
            }
        }
    }
}
