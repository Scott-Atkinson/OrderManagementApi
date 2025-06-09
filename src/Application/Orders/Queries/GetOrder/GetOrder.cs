using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Models;
using Order_Management.Domain.Entities;
using NotFoundException = Order_Management.Application.Common.Exceptions.NotFoundException;

namespace Order_Management.Application.Orders.Queries.GetOrder;

public record GetOrderQuery : IRequest<OrderSummaryDto>
{
    public int OrderId { get; set; }
}

public class GetOrderQueryHandlerManual(IApplicationDbContext context) : IRequestHandler<GetOrderQuery, OrderSummaryDto>
{
    public async Task<OrderSummaryDto> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        return OrderSummaryDto.FromOrder(order!);
    }
}
