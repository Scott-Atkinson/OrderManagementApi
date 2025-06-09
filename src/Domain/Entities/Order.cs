namespace Order_Management.Domain.Entities;
public class Order : IAuditableEntity
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTimeOffset OrderDate { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset ModifiedDate { get; set; }

    // Navigation Properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
