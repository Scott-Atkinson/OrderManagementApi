using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Models;
using NotFoundException = Order_Management.Application.Common.Exceptions.NotFoundException;

namespace Order_Management.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand : UpdateProductDto, IRequest<ProductDto> { }

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // load product from the database
        var product = await _context.Products.FindAsync(request.Id);

        if (product is null)
        {
            throw new NotFoundException(nameof(UpdateProductCommandHandler), request.Id.ToString());
        }

        // update product properties
        product.Name = request!.Name;
        product.Price = request!.Price;
        product.StockQuantity = request!.StockQuantity;

        await _context.SaveChangesAsync(cancellationToken);

        return ProductDto.FromProduct(product);
    }
}
