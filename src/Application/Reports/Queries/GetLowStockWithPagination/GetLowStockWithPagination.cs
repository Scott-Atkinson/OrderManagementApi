using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Mappings;
using Order_Management.Application.Common.Models;

namespace Order_Management.Application.Reports.Queries.GetLowStockWithPagination;

public record GetLowStockWithPaginationQuery : IRequest<PaginatedList<ProductDto>>
{
    public int StockQuantity { get; init; } = 5;
    public int PageNumber { get; init; } = 1;   
    public int PageSize { get; init; } = 10;    
    public string? SearchTerm { get; init; }   
}

public class GetLowStockWithPaginationQueryHandler(IApplicationDbContext context) : IRequestHandler<GetLowStockWithPaginationQuery, PaginatedList<ProductDto>>
{
    public async Task<PaginatedList<ProductDto>> Handle(GetLowStockWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = context.Products
            .Where(x => x.StockQuantity <= request.StockQuantity && x.IsActive)
            .AsQueryable();
        
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(request.SearchTerm));
        }

        query = query.OrderBy(p => p.StockQuantity).ThenBy(p => p.Name);
        
        return await query
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CreatedAt = p.CreatedDate,
                UpdatedAt = p.ModifiedDate
            })
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
