using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Mappings;
using Order_Management.Application.Common.Models;

namespace Order_Management.Application.Orders.Queries.GetOrdersWithPagination;

public record GetOrdersQueryWithPagination : IRequest<PaginatedList<OrderSummaryDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}


public class GetOrdersQueryHandler(IApplicationDbContext context) : IRequestHandler<GetOrdersQueryWithPagination, PaginatedList<OrderSummaryDto>>
{
    public async Task<PaginatedList<OrderSummaryDto>> Handle(GetOrdersQueryWithPagination request, CancellationToken cancellationToken)
    {
        var projectedQuery = context.Orders
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                Status = o.Status,
                CustomerName = o.CustomerName,
                TotalAmount = o.TotalAmount,
                ItemCount = o.OrderItems.Sum(x => x.Quantity),
                Created = o.CreatedDate
            })
            .OrderByDescending(o => o.Id);

        return await projectedQuery.PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
