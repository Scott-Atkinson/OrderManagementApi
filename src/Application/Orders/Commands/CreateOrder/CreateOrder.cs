using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Models;
using Order_Management.Domain.Entities;
using Order_Management.Domain.Enums;

namespace Order_Management.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand : IRequest<OrderSummaryDto>
{
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public List<CreateOrderItemDto> OrderItems { get; init; } = new();
}

public class CreateOrderCommandHandler(IApplicationDbContext context, IOrderNumberService orderNumberService, TimeProvider timeProvider) : IRequestHandler<CreateOrderCommand, OrderSummaryDto>
{
    public async Task<OrderSummaryDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Load products to get prices and update stock
        var productIds = request.OrderItems.Select(x => x.ProductId).ToList();
        var products = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        // Generate order number
        var orderNumber = await orderNumberService.GenerateOrderNumberAsync();

        // Create order items with prices from products
        var orderItems = request.OrderItems.Select(item =>
        {
            var product = products.First(p => p.Id == item.ProductId);
            return new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                Product = product
            };
        }).ToList();

        // Create the order
        var order = new Order
        {
            OrderNumber = orderNumber,
            OrderDate = timeProvider.GetUtcNow().DateTime,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            Status = OrderStatus.Completed,
            OrderItems = orderItems,
            TotalAmount = orderItems.Sum(x => x.Quantity * x.UnitPrice)
        };

        // Update product stock quantities
        foreach (var item in orderItems)
        {
            var product = products.First(p => p.Id == item.ProductId);

            // Race Condition Protection: We already validate this inside the Validator, but another order could have come in just before this order is processed, double-check stock before updating (safety net)
            if (product.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for product '{product.Name} / ID {product.Id}'. Available: {product.StockQuantity}, Required: {item.Quantity}");
            }

            product.StockQuantity -= item.Quantity;

            // Ensure stock never goes negative (additional safety)
            if (product.StockQuantity < 0)
            {
                product.StockQuantity = 0;
            }
        }

        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);

        return OrderSummaryDto.FromOrder(order);
    }
}
