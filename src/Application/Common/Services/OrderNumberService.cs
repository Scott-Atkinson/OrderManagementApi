using Order_Management.Application.Common.Interfaces;

namespace Order_Management.Application.Common.Services;
public class OrderNumberService(IApplicationDbContext context) : IOrderNumberService
{
    public async Task<string> GenerateOrderNumberAsync()
    {
        var today = DateTime.UtcNow;
        var prefix = $"ORD-{today:yyyyMMdd}";

        // Get the last order number for today
        var lastOrderToday = await context.Orders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastOrderToday != null)
        {
            // Extract sequence number from last order (e.g., "ORD-20241207-0001" -> 1)
            var lastSequence = lastOrderToday.OrderNumber.Split('-').LastOrDefault();
            if (int.TryParse(lastSequence, out int parsedSequence))
            {
                sequence = parsedSequence + 1;
            }
        }

        return $"{prefix}-{sequence:D4}"; // ORD-20241207-0001
    }
}
