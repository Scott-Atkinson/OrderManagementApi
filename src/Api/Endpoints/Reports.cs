using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Order_Management.Application.Common.Models;
using Order_Management.Application.Common.Response;
using Order_Management.Application.Reports.Queries.GetDailySummaryWithPagination;
using Order_Management.Application.Reports.Queries.GetLowStockWithPagination;

namespace Order_Management.Api.Endpoints;

/// <summary>
/// Controller for generating business reports and analytics in the order management system.
/// Provides endpoints for daily summaries, inventory reports with caching support.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ReportsController(ISender sender, IMemoryCache memoryCache) : ControllerBase
{
    [HttpGet("daily-summary")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<DailySummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaginatedList<DailySummaryDto>>>> GetTodaysOrdersWithPagination(
        [FromQuery] GetDailySummaryWithPagination query)
    {
        // Create cache key based on query parameters and current date
        var cacheKey = $"daily-orders-{DateTime.UtcNow:yyyy-MM-dd}-{query.PageNumber}-{query.PageSize}-{query.GetHashCode()}";

        // Attempt to retrieve cached results
        if (memoryCache.TryGetValue(cacheKey, out PaginatedList<DailySummaryDto>? cachedResult) && cachedResult != null)
        {
            return ApiResponse<PaginatedList<DailySummaryDto>>.MakeObject(cachedResult, "Data retrieved from cache");
        }

        // Execute query to fetch fresh data from the database
        var result = await sender.Send(query);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.Normal,
        };

        // Store result in cache with configured options
        memoryCache.Set(cacheKey, result, cacheOptions);

        return ApiResponse<PaginatedList<DailySummaryDto>>.MakeObject(result, "Fresh data retrieved and cached");
    }

    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PaginatedList<ProductDto>>>> GetLowStockWithPagination([FromQuery] GetLowStockWithPaginationQuery query)
    {
        var result = await sender.Send(query);

        return ApiResponse<PaginatedList<ProductDto>>.MakeObject(result);
    }
}
