using Order_Management.Domain.Entities;

namespace Order_Management.Application.Common.Models;
public class OrderItemDto
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;

    public string ProductName { get; set; } = string.Empty;

    public DateTimeOffset Created { get; set; }


    // Factory method to create from OrderItem entity
    public static OrderItemDto FromOrderItem(OrderItem orderItem)
    {
        return new OrderItemDto
        {
            Id = orderItem.Id,
            OrderId = orderItem.OrderId,
            ProductId = orderItem.ProductId,
            Quantity = orderItem.Quantity,
            UnitPrice = orderItem.UnitPrice,
            ProductName = orderItem.Product?.Name ?? string.Empty,
            Created = orderItem.CreatedDate
        };
    }
}
