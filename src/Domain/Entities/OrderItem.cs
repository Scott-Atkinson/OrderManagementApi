namespace Order_Management.Domain.Entities;
public class OrderItem : IAuditableEntity
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset ModifiedDate { get; set; }


    // Navigation Properties
    public Order Order { get; set; } = null!;

    public Product Product { get; set; } = null!;
}
