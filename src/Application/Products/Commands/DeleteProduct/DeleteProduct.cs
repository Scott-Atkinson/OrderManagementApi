using Order_Management.Application.Common.Interfaces;
using NotFoundException = Order_Management.Application.Common.Exceptions.NotFoundException;

namespace Order_Management.Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand : IRequest<bool>
{
    public int ProductId { get; set; }
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        // load product
        var product = await _context.Products.FindAsync(request.ProductId);

        if (product is null)
        {
            throw new NotFoundException(nameof(DeleteProductCommandHandler), request.ProductId.ToString());
        }

        // Note: You might want to check if the product is associated with any orders before deleting it.

        product.IsActive = false;

        _context.Products.Update(product);

        var result = await _context.SaveChangesAsync(cancellationToken);

        return result > 0;

    }
}
