using Order_Management.Application.Common.Interfaces;
using Order_Management.Domain.Enums;

namespace Order_Management.Application.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand : IRequest<bool>
{
    public int OrderId { get; set; }
}

public class DeleteOrderCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteOrderCommand, bool>
{
    public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        // Load the order with its items and products
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstAsync(o => o.Id == request.OrderId, cancellationToken);

        // Restore stock quantities for each order item
        foreach (var orderItem in order.OrderItems)
        {
            orderItem.Product.StockQuantity += orderItem.Quantity;
        }

        // Update order status to cancelled
        order.Status = OrderStatus.Cancelled;
        
        // Save changes
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
