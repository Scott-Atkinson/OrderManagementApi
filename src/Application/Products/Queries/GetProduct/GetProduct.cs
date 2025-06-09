using Order_Management.Application.Common.Exceptions;
using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Models;
using NotFoundException = Order_Management.Application.Common.Exceptions.NotFoundException;

namespace Order_Management.Application.Products.Queries.GetProduct;

public record GetProductQuery : IRequest<ProductDto>
{
    public int ProductId { get; set; }
}

public class GetProductQueryHandler(IApplicationDbContext context) : IRequestHandler<GetProductQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
            throw new NotFoundException($"Product with ID {request.ProductId} was not found or it's not active");

        if (!product.IsActive)
            throw new BadRequestException($"Product with ID {request.ProductId} is not available");

        return ProductDto.FromProduct(product!);
    }
}
