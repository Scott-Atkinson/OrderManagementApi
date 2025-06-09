using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Models;
using Order_Management.Domain.Entities;

namespace Order_Management.Application.Products.Commands.CreateProduct;

public record CreateProductCommand : CreateProductDto, IRequest<ProductDto> { }

public class CreateProductCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = new Product
        {
            Name = request!.Name,
            Price = request!.Price,
            StockQuantity = request!.StockQuantity
        };

        context.Products.Add(entity);

        await context.SaveChangesAsync(cancellationToken);

        return ProductDto.FromProduct(entity);
    }
}
