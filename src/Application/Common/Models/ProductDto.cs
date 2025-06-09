using Order_Management.Domain.Entities;

namespace Order_Management.Application.Common.Models;
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    // Factory method to create from Product entity
    public static ProductDto FromProduct(Product order)
    {
        return new ProductDto
        {
            Id = order.Id,
            Name = order.Name,
            Price = order.Price,
            StockQuantity = order.StockQuantity,
            CreatedAt = order.CreatedDate,
            UpdatedAt = order.ModifiedDate
        };
    }
    public static List<ProductDto> CreateListFromProduct(List<Product> product)
    {
        return product.Select(item => new ProductDto
        {
            Id = item.Id,
            Name = item.Name,
            Price = item.Price,
            StockQuantity = item.StockQuantity,
            CreatedAt = item.CreatedDate
        }).ToList();
    }
}
