using Order_Management.Domain.Entities;
using Order_Management.Domain.Enums;

namespace Order_Management.Application.Common.Models;
public class OrderSummaryDto
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public int ItemCount { get; set; }

    public DateTimeOffset OrderDate { get; set; }

    public DateTimeOffset Created { get; set; }

    public ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();

    // Factory method to create from Order entity
    public static OrderSummaryDto FromOrder(Order order)
    {
        return new OrderSummaryDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            Status = order.Status,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            TotalAmount = order.TotalAmount,
            ItemCount = order.OrderItems.Sum(x => x.Quantity),
            Created = order.CreatedDate,
            OrderItems = order.OrderItems.Select(OrderItemDto.FromOrderItem).ToList()
        };
    }
}
