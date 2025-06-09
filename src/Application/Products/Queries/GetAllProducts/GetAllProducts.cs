using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Models;

namespace Order_Management.Application.Products.Queries.GetAllProducts;

public record GetAllProductsQuery : IRequest<List<ProductDto>> { }

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products.ToListAsync(cancellationToken);
        return ProductDto.CreateListFromProduct(products);
    }
}
