using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Models;
using Order_Management.Domain.Entities;

namespace Order_Management.Application.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand : UpdateOrderDto, IRequest<OrderSummaryDto>
{
    public int OrderId { get; set; }
}

public class UpdateOrderCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateOrderCommand, OrderSummaryDto>
{
    public async Task<OrderSummaryDto> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        // Load the existing order with all related data
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstAsync(o => o.Id == request.OrderId, cancellationToken);

        // Update basic order properties
        order.CustomerName = request.CustomerName;
        order.CustomerEmail = request.CustomerEmail;
        order.Status = request.Status;

        // Process order items changes
        await ProcessOrderItemsChanges(order, request.OrderItems.ToList(), cancellationToken);

        // Recalculate total amount
        order.TotalAmount = order.OrderItems.Sum(x => x.Quantity * x.UnitPrice);

        // Save changes
        await context.SaveChangesAsync(cancellationToken);

        // Return optimized summary without additional database query
        return new OrderSummaryDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            Status = order.Status,
            CustomerName = order.CustomerName,
            TotalAmount = order.TotalAmount,
            ItemCount = order.OrderItems.Sum(x => x.Quantity),
            Created = order.CreatedDate
        };
    }

    private async Task ProcessOrderItemsChanges(Order order, List<UpdateOrderItemDto> requestItems, CancellationToken cancellationToken)
    {
        var currentItems = order.OrderItems.ToList();

        // Get all product IDs we'll need (including existing ones for potential updates)
        var requestProductIds = requestItems.Select(x => x.ProductId).Distinct().ToList();
        var existingProductIds = currentItems.Select(x => x.ProductId).Distinct().ToList();
        var allProductIds = requestProductIds.Union(existingProductIds).ToList();

        // Create lookup dictionary for efficient product access
        var products = await context.Products
            .Where(p => allProductIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var productLookup = products.ToDictionary(p => p.Id);

        // Get submitted item IDs for efficient lookup
        var submittedItemIds = requestItems
            .Where(x => x.Id.HasValue)
            .Select(x => x.Id!.Value)
            .ToHashSet();

        // Create lookup for request items by ID for efficient access
        var requestItemLookup = requestItems
            .Where(x => x.Id.HasValue)
            .ToDictionary(x => x.Id!.Value);

        // Process existing items - determine which to update/delete
        var itemsToRemove = new List<OrderItem>();

        foreach (var currentItem in currentItems)
        {
            // If the item ID is not in the submission, mark for deletion
            if (!submittedItemIds.Contains(currentItem.Id))
            {
                // Item is being removed - restore full quantity to stock
                productLookup[currentItem.ProductId].StockQuantity += currentItem.Quantity;
                itemsToRemove.Add(currentItem);
            }
            else if (requestItemLookup.TryGetValue(currentItem.Id, out var requestItem))
            {
                // Item is being updated - check if quantity changed
                var quantityDifference = requestItem.Quantity - currentItem.Quantity;

                if (quantityDifference != 0)
                {
                    var product = productLookup[currentItem.ProductId];

                    if (quantityDifference > 0)
                    {
                        // Customer wants more items - need to reserve additional stock
                        if (product.StockQuantity < quantityDifference)
                        {
                            throw new ValidationException($"Insufficient stock for product '{product.Name} / ID {product.Id}'. Available: {product.StockQuantity}, Additional needed: {quantityDifference}");
                        }

                        // Reserve additional stock
                        product.StockQuantity -= quantityDifference;
                    }
                    else
                    {
                        // Customer wants fewer items - restore stock
                        var quantityToRestore = Math.Abs(quantityDifference);
                        product.StockQuantity += quantityToRestore;
                    }

                    // Update the item with new quantity and current price
                    currentItem.Quantity = requestItem.Quantity;
                    currentItem.UnitPrice = product.Price;
                }
            }
        }

        // Remove items marked for deletion
        foreach (var itemToRemove in itemsToRemove)
        {
            order.OrderItems.Remove(itemToRemove);
            context.OrderItems.Remove(itemToRemove);
        }

        // Process new items (those without an Id)
        var newItems = requestItems.Where(x => !x.Id.HasValue).ToList();
        foreach (var newItem in newItems)
        {
            var product = productLookup[newItem.ProductId];

            // Check stock availability for new item
            if (product.StockQuantity < newItem.Quantity)
            {
                throw new ValidationException($"Insufficient stock for product '{product.Name} / ID {product.Id}'. Available: {product.StockQuantity}, Requested: {newItem.Quantity}");
            }

            // Reserve stock for new item
            product.StockQuantity -= newItem.Quantity;

            // Create new order item
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = newItem.ProductId,
                Quantity = newItem.Quantity,
                UnitPrice = product.Price,
                Product = product
            };

            // Add to order
            order.OrderItems.Add(orderItem);
        }
    }
}
