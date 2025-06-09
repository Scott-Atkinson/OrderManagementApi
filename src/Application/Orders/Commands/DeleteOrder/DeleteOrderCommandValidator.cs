using Order_Management.Application.Common.Interfaces;
using Order_Management.Domain.Entities;
using Order_Management.Domain.Enums;
using NotFoundException = Order_Management.Application.Common.Exceptions.NotFoundException;

namespace Order_Management.Application.Orders.Commands.DeleteOrder;

public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
{
    private readonly IApplicationDbContext _context;

    // Applying Business Logic on delete, only allow deletion for specific statuses
    private static readonly OrderStatus[] DeletableStatuses = {
        OrderStatus.Pending,
        OrderStatus.Processing
    };

    public DeleteOrderCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.OrderId)
            .GreaterThan(0)
            .WithMessage("Order ID must be greater than 0.");

        RuleFor(x => x.OrderId)
            .MustAsync(OrderExistsAndCanBeDeleted)
            .WithMessage("Order not found or cannot be deleted.");
    }

    private async Task<bool> OrderExistsAndCanBeDeleted(int orderId, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException(nameof(Order), orderId.ToString());
        }

        // Check if order status allows deletion
        if (!DeletableStatuses.Contains(order.Status))
        {
            var allowedStatuses = string.Join(", ", DeletableStatuses);
            throw new ValidationException($"Cannot delete order {order.OrderNumber}. Order status '{order.Status}' does not allow deletion. Allowed statuses: {allowedStatuses}");
        }

        return true;
    }
}
