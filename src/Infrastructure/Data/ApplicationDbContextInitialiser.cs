using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Order_Management.Domain.Entities;
using Order_Management.Domain.Enums;

namespace Order_Management.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context)
{
    public async Task InitialiseAsync()
    {
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {

        if (!await context.Products.AnyAsync())
        {
            var products = new List<Product>
    {
        new Product
        {
            Id = 1, // Explicitly set ID
            Name = "Wireless Bluetooth Headphones",
            Price = 89.99m,
            StockQuantity = 25,
            IsActive = true
        },
        new Product
        {
            Id = 2,
            Name = "Stainless Steel Water Bottle",
            Price = 24.99m,
            StockQuantity = 50,
            IsActive = true
        },
        new Product
        {
            Id = 3,
            Name = "Mechanical Gaming Keyboard",
            Price = 129.99m,
            StockQuantity = 15,
            IsActive = true
        },
        new Product
        {
            Id = 4,
            Name = "Organic Cotton T-Shirt",
            Price = 19.99m,
            StockQuantity = 100,
            IsActive = true
        },
        new Product
        {
            Id = 5,
            Name = "LED Desk Lamp",
            Price = 45.99m,
            StockQuantity = 30,
            IsActive = true
        },
        new Product
        {
            Id = 6,
            Name = "Portable Phone Charger",
            Price = 29.99m,
            StockQuantity = 75,
            IsActive = true
        },
        new Product
        {
            Id = 7,
            Name = "Coffee Bean Grinder",
            Price = 79.99m,
            StockQuantity = 20,
            IsActive = true
        },
        new Product
        {
            Id = 8,
            Name = "Yoga Exercise Mat",
            Price = 34.99m,
            StockQuantity = 40,
            IsActive = true
        },
        new Product
        {
            Id = 9,
            Name = "Wireless Computer Mouse",
            Price = 39.99m,
            StockQuantity = 60,
            IsActive = true
        },
        new Product
        {
            Id = 10,
            Name = "Ceramic Coffee Mug Set",
            Price = 22.99m,
            StockQuantity = 35,
            IsActive = true
        },
        new Product
        {
            Id = 11,
            Name = "Bluetooth Fitness Tracker",
            Price = 149.99m,
            StockQuantity = 12,
            IsActive = true
        },
        new Product
        {
            Id = 12,
            Name = "Bamboo Cutting Board",
            Price = 18.99m,
            StockQuantity = 45,
            IsActive = true
        },
        new Product
        {
            Id = 13,
            Name = "USB-C Hub Adapter",
            Price = 54.99m,
            StockQuantity = 28,
            IsActive = true
        },
        new Product
        {
            Id = 14,
            Name = "Insulated Lunch Box",
            Price = 16.99m,
            StockQuantity = 65,
            IsActive = false // Example of inactive product
        },
        new Product
        {
            Id = 15,
            Name = "Wireless Phone Stand",
            Price = 32.99m,
            StockQuantity = 0, // Example of out-of-stock product
            IsActive = true
        },
        new Product
        {
            Id = 16,
            Name = "Premium Leather Wallet",
            Price = 68.99m,
            StockQuantity = 3,
            IsActive = true
        },
        new Product
        {
            Id = 17,
            Name = "Smart Watch Band",
            Price = 25.99m,
            StockQuantity = 2,
            IsActive = true
        },
        new Product
        {
            Id = 18,
            Name = "Wireless Earbuds Case",
            Price = 15.99m,
            StockQuantity = 4,
            IsActive = true
        },
        new Product
        {
            Id = 19,
            Name = "Gaming Mouse Pad XXL",
            Price = 42.99m,
            StockQuantity = 1,
            IsActive = true
        },
        new Product
        {
            Id = 20,
            Name = "Titanium Phone Case",
            Price = 89.99m,
            StockQuantity = 2,
            IsActive = true
        },
        new Product
        {
            Id = 21,
            Name = "Portable SSD Drive",
            Price = 199.99m,
            StockQuantity = 1,
            IsActive = true
        },
        new Product
        {
            Id = 22,
            Name = "Ergonomic Wrist Rest",
            Price = 28.99m,
            StockQuantity = 4,
            IsActive = true
        },
        new Product
        {
            Id = 23,
            Name = "Retro Vinyl Record",
            Price = 34.99m,
            StockQuantity = 3,
            IsActive = true
        }
    };

            context.Products.AddRange(products);
            await context.SaveChangesAsync(); // Save products first
        }

        if (!await context.Orders.AnyAsync())
        {
            var orders = new List<Order>
    {
        new Order
        {
            Id = 1, // Explicitly set Order ID too
            OrderNumber = "ORD-2025-001",
            Status = OrderStatus.Completed,
            CustomerName = "John Smith",
            CustomerEmail = "john.smith@email.com",
            TotalAmount = 159.97m,
            OrderDate = DateTime.Now.AddHours(-2),
            OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 1, UnitPrice = 89.99m }, // Wireless Bluetooth Headphones
                new OrderItem { ProductId = 5, Quantity = 1, UnitPrice = 45.99m }, // LED Desk Lamp
                new OrderItem { ProductId = 4, Quantity = 1, UnitPrice = 19.99m }  // Organic Cotton T-Shirt
            }
        },
        new Order
        {
            Id = 2,
            OrderNumber = "ORD-2025-002",
            Status = OrderStatus.Shipped,
            CustomerName = "Sarah Johnson",
            CustomerEmail = "sarah.j@gmail.com",
            TotalAmount = 209.97m, // Fixed calculation: 129.99 + (2 * 39.99) = 209.97
            OrderDate = DateTime.Now.AddHours(-4),
            OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 3, Quantity = 1, UnitPrice = 129.99m }, // Mechanical Gaming Keyboard
                new OrderItem { ProductId = 9, Quantity = 2, UnitPrice = 39.99m }   // Wireless Computer Mouse (2x)
            }
        },
        new Order
        {
            Id = 3,
            OrderNumber = "ORD-2025-003",
            Status = OrderStatus.Processing,
            CustomerName = "Mike Wilson",
            CustomerEmail = "mike.wilson@company.com",
            TotalAmount = 324.97m, // Fixed calculation: (2 * 149.99) + 24.99 = 324.97
            OrderDate = DateTime.Now.AddHours(-1),
            OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 11, Quantity = 2, UnitPrice = 149.99m }, // Bluetooth Fitness Tracker (2x)
                new OrderItem { ProductId = 2, Quantity = 1, UnitPrice = 24.99m }    // Stainless Steel Water Bottle
            }
        },
        new Order
        {
            Id = 4,
            OrderNumber = "ORD-2025-004",
            Status = OrderStatus.Pending,
            CustomerName = "Emily Davis",
            CustomerEmail = "emily.davis@outlook.com",
            TotalAmount = 102.98m, // Fixed calculation: 79.99 + 22.99 = 102.98
            OrderDate = DateTime.Now.AddMinutes(-30),
            OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 7, Quantity = 1, UnitPrice = 79.99m }, // Coffee Bean Grinder
                new OrderItem { ProductId = 10, Quantity = 1, UnitPrice = 22.99m } // Ceramic Coffee Mug Set
            }
        },
        new Order
        {
            Id = 5,
            OrderNumber = "ORD-2025-005",
            Status = OrderStatus.Completed,
            CustomerName = "David Brown",
            CustomerEmail = "david.brown@email.com",
            TotalAmount = 170.94m, // Fixed calculation: (2 * 34.99) + 29.99 + (4 * 18.99) = 170.94
            OrderDate = DateTime.Now.AddHours(-6),
            OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 8, Quantity = 2, UnitPrice = 34.99m }, // Yoga Exercise Mat (2x)
                new OrderItem { ProductId = 6, Quantity = 1, UnitPrice = 29.99m }, // Portable Phone Charger
                new OrderItem { ProductId = 12, Quantity = 4, UnitPrice = 18.99m } // Bamboo Cutting Board (4x)
            }
        },
        new Order
        {
            Id = 6,
            OrderNumber = "ORD-2025-006",
            Status = OrderStatus.Cancelled,
            CustomerName = "Lisa Anderson",
            CustomerEmail = "lisa.anderson@gmail.com",
            TotalAmount = 54.99m,
            OrderDate = DateTime.Now.AddHours(-3),
            OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 13, Quantity = 1, UnitPrice = 54.99m } // USB-C Hub Adapter
            }
        },
        new Order
        {
            Id = 7,
            OrderNumber = "ORD-2025-007",
            Status = OrderStatus.Processing,
            CustomerName = "Robert Taylor",
            CustomerEmail = "robert.taylor@company.org",
            TotalAmount = 170.95m, // Fixed calculation: 25.99 + (3 * 15.99) + 68.99 + (2 * 18.99) = 170.95
            OrderDate = DateTime.Now.AddMinutes(-45),
            OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 17, Quantity = 1, UnitPrice = 25.99m }, // Smart Watch Band (corrected ProductId)
                new OrderItem { ProductId = 18, Quantity = 3, UnitPrice = 15.99m }, // Wireless Earbuds Case (3x)
                new OrderItem { ProductId = 16, Quantity = 1, UnitPrice = 68.99m }, // Premium Leather Wallet (corrected ProductId)
                new OrderItem { ProductId = 12, Quantity = 2, UnitPrice = 18.99m }  // Bamboo Cutting Board (2x)
            }
        },
        new Order
        {
            Id = 8,
            OrderNumber = "ORD-2025-008",
            Status = OrderStatus.Shipped,
            CustomerName = "Jennifer White",
            CustomerEmail = "jen.white@email.com",
            TotalAmount = 92.97m, // Fixed calculation: (2 * 28.99) + 34.99 = 92.97
            OrderDate = DateTime.Now.AddHours(-5),
            OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 22, Quantity = 2, UnitPrice = 28.99m }, // Ergonomic Wrist Rest (corrected ProductId)
                new OrderItem { ProductId = 23, Quantity = 1, UnitPrice = 34.99m }, // Retro Vinyl Record (corrected ProductId)
                new OrderItem { ProductId = 4, Quantity = 1, UnitPrice = 19.99m }   // Organic Cotton T-Shirt
            }
        }
    };

            context.Orders.AddRange(orders);
            await context.SaveChangesAsync(); // Save orders and order items
        }

    }
}
