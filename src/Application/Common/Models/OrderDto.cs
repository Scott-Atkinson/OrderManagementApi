using Order_Management.Domain.Enums;

namespace Order_Management.Application.Common.Models;
public class OrderDto
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTimeOffset OrderDate { get; set; }

    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
}
