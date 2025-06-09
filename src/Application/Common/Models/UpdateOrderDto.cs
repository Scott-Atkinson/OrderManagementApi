using Order_Management.Domain.Enums;

namespace Order_Management.Application.Common.Models;

public record UpdateOrderDto
{
    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public ICollection<UpdateOrderItemDto> OrderItems { get; set; } = new List<UpdateOrderItemDto>();
}
