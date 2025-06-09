using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Models;
using Order_Management.Domain.Enums;

namespace Order_Management.Application.Reports.Queries.GetDailySummaryWithPagination;

public record GetDailySummaryWithPagination : IRequest<PaginatedList<DailySummaryDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetDailySummaryWithPaginationQueryHandler(IApplicationDbContext context, TimeProvider timeProvider) : IRequestHandler<GetDailySummaryWithPagination, PaginatedList<DailySummaryDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly TimeProvider _timeProvider = timeProvider;

    // This needs to be improved, the query needs to take into consideration the users timezone to ensure data is returned
    // For this example I am filtering on the client, which in the long term as the database grows bad for performance.
    public async Task<PaginatedList<DailySummaryDto>> Handle(GetDailySummaryWithPagination request, CancellationToken cancellationToken)
    {
        var allOrders = await _context.Orders
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Get Melbourne timezone info
        var melbourneTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Australia/Melbourne");
        var nowInMelbourne = TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), melbourneTimeZone);
        var todayInMelbourne = nowInMelbourne.Date;

        var todaysOrders = allOrders.Where(order =>
        {
            var orderInMelbourne = TimeZoneInfo.ConvertTime(order.OrderDate, melbourneTimeZone);
            var orderDateOnly = orderInMelbourne.Date;
            return orderDateOnly == todayInMelbourne;
        }).ToList();

        var summary = new DailySummaryDto
        {
            Date = todayInMelbourne,
            TotalOrders = todaysOrders.Count,
            TotalRevenue = todaysOrders.Sum(o => o.TotalAmount),
            PendingOrders = todaysOrders.Count(o => o.Status == OrderStatus.Pending),
            ProcessingOrders = todaysOrders.Count(o => o.Status == OrderStatus.Processing),
            CompletedOrders = todaysOrders.Count(o => o.Status == OrderStatus.Completed),
            ShippedOrders = todaysOrders.Count(o => o.Status == OrderStatus.Shipped),
            CancelledOrders = todaysOrders.Count(o => o.Status == OrderStatus.Cancelled)
        };

        var summaryList = new List<DailySummaryDto> { summary };
        var totalCount = summaryList.Count;
        var items = summaryList
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedList<DailySummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
