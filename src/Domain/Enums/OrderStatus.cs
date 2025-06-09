namespace Order_Management.Domain.Enums;

// For simplicity, any order that comes in we mark as complete
public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Shipped = 3,
    Completed = 4,
    Cancelled = 5
}
