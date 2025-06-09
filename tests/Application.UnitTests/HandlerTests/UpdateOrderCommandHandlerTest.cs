using Application.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Order_Management.Application.Common.Models;
using Order_Management.Application.Orders.Commands.UpdateOrder;
using Order_Management.Domain.Entities;
using Order_Management.Domain.Enums;

namespace Application.UnitTests.HandlerTests;

public class UpdateOrderCommandHandlerTest
{

    [Fact]
    public async Task UpdateOrder_Validation_ShouldPass_WithValidData()
    {
        // Arrange
        var context = TestDbContextFactory.CreateInMemoryContext();

        var product = new Product
        {
            Id = 1,
            Name = "Wireless Mouse",
            Price = 39.99m,
            StockQuantity = 10,
            IsActive = true
        };

        var order = new Order
        {
            Id = 123,
            OrderNumber = "ORD-0001",
            CustomerName = "Jane Doe",
            CustomerEmail = "jane@example.com",
            Status = OrderStatus.Pending,
            OrderDate = DateTimeOffset.Now,
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = 10,
                    OrderId = 123,
                    ProductId = 1,
                    Quantity = 5,
                    UnitPrice = 39.99m
                }
            }
        };

        context.Products.Add(product);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var command = new UpdateOrderCommand
        {
            OrderId = order.Id,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            Status = order.Status,
            OrderItems = new List<UpdateOrderItemDto>
            {
                new UpdateOrderItemDto
                {
                    Id = 10,
                    ProductId = 1,
                    Quantity = 2
                }
            }
        };

        var validator = new UpdateOrderCommandValidator(context);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task UpdateOrder_WhenOrderStatusIsCompleted_ShouldReturnValidationError()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();

        // Arrange
        var completedOrder = new Order
        {
            Id = 1,
            OrderNumber = "ORD-2025-001",
            Status = OrderStatus.Completed, // Order is completed - should NOT be updatable
            CustomerName = "John Doe",
            CustomerEmail = "john.doe@email.com",
            TotalAmount = 100.00m,
            OrderDate = DateTimeOffset.Now.AddDays(-1),
            OrderItems = new List<OrderItem>
        {
            new OrderItem
            {
                Id = 1,
                OrderId = 1,
                ProductId = 1,
                Quantity = 2,
                UnitPrice = 50.00m
            }
        }
        };

        var activeProduct = new Product
        {
            Id = 1,
            Name = "Test Product",
            Price = 50.00m,
            StockQuantity = 10,
            IsActive = true
        };

        // Add entities to in-memory database
        context.Orders.Add(completedOrder);
        context.Products.Add(activeProduct);
        await context.SaveChangesAsync();

        // Create update command
        var command = new UpdateOrderCommand
        {
            OrderId = 1,
            CustomerName = "John Doe Updated",
            CustomerEmail = "john.updated@email.com",
            Status = OrderStatus.Processing, // Trying to change status
            OrderItems = new List<UpdateOrderItemDto>
        {
            new UpdateOrderItemDto
            {
                Id = 1,
                ProductId = 1,
                Quantity = 3 // Trying to increase quantity
            }
        }
        };

        var validator = new UpdateOrderCommandValidator(context);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(nameof(command.OrderId), result.Errors.First().PropertyName);
        Assert.Contains("Cannot update order ORD-2025-001", result.Errors.First().ErrorMessage);
        Assert.Contains("Only pending and processing orders can be updated", result.Errors.First().ErrorMessage);
        Assert.Contains("Current status: Completed", result.Errors.First().ErrorMessage);
    }

    [Fact]
    public async Task UpdateOrder_WhenOrderStatusIsPending_ShouldAllowUpdate()
    {

        var context = TestDbContextFactory.CreateInMemoryContext();

        // Arrange
        var pendingOrder = new Order
        {
            Id = 4,
            OrderNumber = "ORD-2025-004",
            Status = OrderStatus.Pending, // Order is pending - SHOULD be updatable
            CustomerName = "Alice Wilson",
            CustomerEmail = "alice.wilson@email.com",
            TotalAmount = 50.00m,
            OrderDate = DateTimeOffset.Now.AddHours(-2),
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = 4,
                    OrderId = 4,
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 50.00m
                }
            }
        };

        var activeProduct = new Product
        {
            Id = 1,
            Name = "Test Product",
            Price = 50.00m,
            StockQuantity = 10,
            IsActive = true
        };

        // Add entities to in-memory database
        context.Orders.Add(pendingOrder);
        context.Products.Add(activeProduct);
        await context.SaveChangesAsync();

        // Create update command
        var command = new UpdateOrderCommand
        {
            OrderId = 4,
            CustomerName = "Alice Wilson Updated",
            CustomerEmail = "alice.updated@email.com",
            Status = OrderStatus.Processing, // Valid status change
            OrderItems = new List<UpdateOrderItemDto>
            {
                new UpdateOrderItemDto
                {
                    Id = 4,
                    ProductId = 1,
                    Quantity = 2 // Valid quantity increase
                }
            }
        };

        var validator = new UpdateOrderCommandValidator(context);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task UpdateOrder_ShouldIncreaseProductStock_WhenOrderItemQuantityIsDecreased()
    {
        // Arrange
        var context = TestDbContextFactory.CreateInMemoryContext();

        var product = new Product
        {
            Id = 1,
            Name = "Wireless Mouse",
            Price = 39.99m,
            StockQuantity = 10,
            IsActive = true 
        };

        var order = new Order
        {
            Id = 123,
            OrderNumber = "ORD-0001",
            CustomerName = "Jane Doe",
            CustomerEmail = "jane@example.com",
            Status = OrderStatus.Pending,
            OrderDate = DateTimeOffset.Now, 
            OrderItems = new List<OrderItem>
        {
            new OrderItem
            {
                Id = 10,
                OrderId = 123,
                ProductId = 1,
                Quantity = 5,
                UnitPrice = 39.99m
            }
        }
        };

        context.Products.Add(product);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var command = new UpdateOrderCommand
        {
            OrderId = order.Id,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            Status = order.Status,
            OrderItems = new List<UpdateOrderItemDto>
        {
            new UpdateOrderItemDto
            {
                Id = 10,
                ProductId = 1,
                Quantity = 2 // Decreased from 5 to 2
            }
        }
        };

        // First validate the command to ensure it passes validation
        var validator = new UpdateOrderCommandValidator(context);
        var validationResult = await validator.ValidateAsync(command);

        // Debug validation errors if any
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new InvalidOperationException($"Validation failed: {errors}");
        }

        var handler = new UpdateOrderCommandHandler(context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedProduct = await context.Products.FirstAsync(p => p.Id == 1);

        // Stock should increase by 3 (5 original - 2 new = 3 returned to stock)
        // Original stock: 10, Reserved: 5, New reserved: 2, Final stock: 10 + 3 = 13
        Assert.Equal(13, updatedProduct.StockQuantity);
        Assert.Equal(2, result.ItemCount); // Total quantity in order
        Assert.Equal(2 * product.Price, result.TotalAmount);
    }

    [Fact]
    public async Task UpdateOrder_ShouldDecreaseProductStock_WhenOrderItemQuantityIsIncreased()
    {
        // Arrange
        var context = TestDbContextFactory.CreateInMemoryContext();

        var product = new Product
        {
            Id = 1,
            Name = "Wireless Mouse",
            Price = 39.99m,
            StockQuantity = 10,
            IsActive = true // Required by validator!
        };

        var order = new Order
        {
            Id = 123,
            OrderNumber = "ORD-0001",
            CustomerName = "Jane Doe",
            CustomerEmail = "jane@example.com",
            Status = OrderStatus.Pending,
            OrderDate = DateTimeOffset.Now, // Add this if required
            OrderItems = new List<OrderItem>
        {
            new OrderItem
            {
                Id = 10,
                OrderId = 123, // Must match the order ID
                ProductId = 1,
                Quantity = 2, // existing quantity
                UnitPrice = 39.99m
            }
        }
        };

        context.Products.Add(product);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var command = new UpdateOrderCommand
        {
            OrderId = order.Id,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            Status = order.Status,
            OrderItems = new List<UpdateOrderItemDto>
        {
            new UpdateOrderItemDto
            {
                Id = 10,
                ProductId = 1,
                Quantity = 5 // increased from 2 → 5 (should reduce stock by 3)
            }
        }
        };

        // First validate the command to ensure it passes validation
        var validator = new UpdateOrderCommandValidator(context);
        var validationResult = await validator.ValidateAsync(command);

        // Debug validation errors if any
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new InvalidOperationException($"Validation failed: {errors}");
        }

        var handler = new UpdateOrderCommandHandler(context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedProduct = await context.Products.FirstAsync(p => p.Id == 1);

        // Stock should decrease by 3 (5 new - 2 original = 3 additional reserved)
        // Original stock: 10, Originally reserved: 2, Additional reserved: 3, Final stock: 10 - 3 = 7
        Assert.Equal(7, updatedProduct.StockQuantity);
        Assert.Equal(5, result.ItemCount); // Total quantity in order
        Assert.Equal(5 * product.Price, result.TotalAmount); // Total recalculated
    }
    
  
}

