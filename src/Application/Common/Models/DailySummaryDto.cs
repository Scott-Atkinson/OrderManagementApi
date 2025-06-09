namespace Order_Management.Application.Common.Models;
public class DailySummaryDto
{
    public DateTime Date { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int CancelledOrders { get; set; }

    // Computed properties
    public decimal AverageOrderValue => TotalOrders > 0 ? Math.Round(TotalRevenue / TotalOrders, 2) : 0;


}
