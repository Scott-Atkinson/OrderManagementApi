namespace Order_Management.Application.Common.Models;
public class UpdateOrderItemDto
{
    public int? Id { get; set; } // Null for new products being added
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
