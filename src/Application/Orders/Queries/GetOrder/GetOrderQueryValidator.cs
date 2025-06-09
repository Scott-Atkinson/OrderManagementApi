using Order_Management.Application.Common.Interfaces;
using NotFoundException = Order_Management.Application.Common.Exceptions.NotFoundException;

namespace Order_Management.Application.Orders.Queries.GetOrder;

public class GetOrderQueryValidator : AbstractValidator<GetOrderQuery>
{
    private readonly IApplicationDbContext _context;
    public GetOrderQueryValidator(IApplicationDbContext context)
    {
        _context = context;
        RuleFor(x => x.OrderId)
            .GreaterThan(0)
            .WithMessage("Order ID must be greater than 0.");

        RuleFor(x => x.OrderId)
            .MustAsync(ValidateOrderExists);
    }

    private async Task<bool> ValidateOrderExists(int orderId, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException($"Order with ID {orderId} was not found");
        }

        return true;

    }
}
