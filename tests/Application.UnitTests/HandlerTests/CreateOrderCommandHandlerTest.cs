using Application.UnitTests.MockExtensions;
using Application.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Models;
using Order_Management.Application.Orders.Commands.CreateOrder;
using Order_Management.Domain.Entities;
using Order_Management.Domain.Enums;

namespace Application.UnitTests.HandlerTests;

public class CreateOrderCommandHandlerTests
{

    [Fact]
    public async Task CreateOrder_ShouldCreateOrderSuccessfully_WithValidData()
    {
        // Arrange
        var context = TestDbContextFactory.CreateInMemoryContext();

        var product1 = new Product
        {
            Id = 1,
            Name = "Wireless Mouse",
            Price = 39.99m,
            StockQuantity = 20,
            IsActive = true
        };

        var product2 = new Product
        {
            Id = 2,
            Name = "USB Keyboard",
            Price = 59.99m,
            StockQuantity = 15,
            IsActive = true
        };

        context.Products.Add(product1);
        context.Products.Add(product2);
        await context.SaveChangesAsync();

        var command = new CreateOrderCommand
        {
            CustomerName = "John Smith",
            CustomerEmail = "john.smith@email.com",
            OrderItems = new List<CreateOrderItemDto>
        {
            new CreateOrderItemDto
            {
                ProductId = 1,
                Quantity = 3
            },
            new CreateOrderItemDto
            {
                ProductId = 2,
                Quantity = 2
            }
        }
        };

        // Mock services
        var mockOrderNumberService = new Mock<IOrderNumberService>();
        mockOrderNumberService.Setup(x => x.GenerateOrderNumberAsync())
                              .ReturnsAsync("ORD-2025-001");

        var mockTimeProvider = new Mock<TimeProvider>();
        mockTimeProvider.Setup(x => x.GetUtcNow())
                       .Returns(new DateTimeOffset(2025, 6, 9, 10, 0, 0, TimeSpan.Zero));

        // Validate the command first
        var validator = new CreateOrderCommandValidator(context);
        var validationResult = await validator.ValidateAsync(command);

        // Debug validation errors if any
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new InvalidOperationException($"Validation failed: {errors}");
        }

        var handler = new CreateOrderCommandHandler(context, mockOrderNumberService.Object, mockTimeProvider.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert

        // Verify order was created
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("ORD-2025-001", result.OrderNumber);
        Assert.Equal("John Smith", result.CustomerName);
        Assert.Equal("john.smith@email.com", result.CustomerEmail);
        Assert.Equal(OrderStatus.Completed, result.Status); // Note: Handler sets to Completed, consider changing to Pending

        // Verify order items
        Assert.Equal(2, result.OrderItems.Count);
        Assert.Equal(5, result.ItemCount); // 3 + 2 = 5 total items

        // Verify total amount calculation
        var expectedTotal = (3 * 39.99m) + (2 * 59.99m); // 119.97 + 119.98 = 239.95
        Assert.Equal(expectedTotal, result.TotalAmount);

        // Verify order persisted in database
        var savedOrder = await context.Orders
            .Include(o => o.OrderItems)
            .FirstAsync(o => o.Id == result.Id);

        Assert.Equal("John Smith", savedOrder.CustomerName);
        Assert.Equal("john.smith@email.com", savedOrder.CustomerEmail);
        Assert.Equal(OrderStatus.Completed, savedOrder.Status);
        Assert.Equal(2, savedOrder.OrderItems.Count);
        Assert.Equal(expectedTotal, savedOrder.TotalAmount);

        // Verify stock was reduced correctly
        var updatedProduct1 = await context.Products.FirstAsync(p => p.Id == 1);
        var updatedProduct2 = await context.Products.FirstAsync(p => p.Id == 2);

        Assert.Equal(17, updatedProduct1.StockQuantity); // 20 - 3 = 17
        Assert.Equal(13, updatedProduct2.StockQuantity); // 15 - 2 = 13

        // Verify order items have correct data
        var orderItem1 = savedOrder.OrderItems.First(oi => oi.ProductId == 1);
        var orderItem2 = savedOrder.OrderItems.First(oi => oi.ProductId == 2);

        Assert.Equal(3, orderItem1.Quantity);
        Assert.Equal(39.99m, orderItem1.UnitPrice);
        Assert.Equal(savedOrder.Id, orderItem1.OrderId);

        Assert.Equal(2, orderItem2.Quantity);
        Assert.Equal(59.99m, orderItem2.UnitPrice);
        Assert.Equal(savedOrder.Id, orderItem2.OrderId);

        // Verify services were called
        mockOrderNumberService.Verify(x => x.GenerateOrderNumberAsync(), Times.Once);
        mockTimeProvider.Verify(x => x.GetUtcNow(), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_ShouldThrowInvalidOperationException_WhenQuantityExceedsStock()
    {
        // Arrange
        var productId = 1;
        var mockProducts = new List<Product>
        {
            new Product
            {
                Id = productId,
                Name = "Wireless Bluetooth Headphones",
                Price = 89.99m,
                StockQuantity = 5, // low stock
                IsActive = true
            }
        };

        var orderItems = new List<CreateOrderItemDto>
        {
            new CreateOrderItemDto
            {
                ProductId = productId,
                Quantity = 10 // exceeds stock
            }
        };

        var mockDbSet = mockProducts.ToMockDbSet();

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Products).Returns(mockDbSet.Object);
        mockContext.Setup(c => c.Orders.Add(It.IsAny<Order>()));

        var mockOrderNumberService = new Mock<IOrderNumberService>();
        mockOrderNumberService.Setup(s => s.GenerateOrderNumberAsync())
            .ReturnsAsync("ORD-0001");

        var timeProvider = TimeProvider.System;

        var handler = new CreateOrderCommandHandler(mockContext.Object, mockOrderNumberService.Object, timeProvider);

        var command = new CreateOrderCommand
        {
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com",
            OrderItems = orderItems
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Contains("Insufficient stock", ex.Message);
    }

    [Fact]
    public async Task CreateOrder_ShouldHandleRaceConditionGracefully_WhenStockBecomesInsufficient()
    {
        // Arrange
        var context = TestDbContextFactory.CreateInMemoryContext();

        var product = new Product
        {
            Id = 1,
            Name = "Limited Item",
            Price = 100.00m,
            StockQuantity = 2, // Very low stock to test race condition
            IsActive = true
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var command = new CreateOrderCommand
        {
            CustomerName = "Race Test User",
            CustomerEmail = "race@test.com",
            OrderItems = new List<CreateOrderItemDto>
        {
            new CreateOrderItemDto
            {
                ProductId = 1,
                Quantity = 1
            }
        }
        };

        // Mock services
        var mockOrderNumberService = new Mock<IOrderNumberService>();
        mockOrderNumberService.Setup(x => x.GenerateOrderNumberAsync())
                              .ReturnsAsync("ORD-2025-003");

        var mockTimeProvider = new Mock<TimeProvider>();
        mockTimeProvider.Setup(x => x.GetUtcNow())
                       .Returns(DateTimeOffset.UtcNow);

        var handler = new CreateOrderCommandHandler(context, mockOrderNumberService.Object, mockTimeProvider.Object);

        // Simulate race condition by reducing stock after validation but before handler execution
        // First order should succeed
        var result1 = await handler.Handle(command, CancellationToken.None);
        Assert.NotNull(result1);

        // Manually reduce stock to 0 to simulate another order being processed
        var productToUpdate = await context.Products.FirstAsync(p => p.Id == 1);
        productToUpdate.StockQuantity = 0;
        await context.SaveChangesAsync();

        // Second order should fail with race condition protection
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        Assert.Contains("Insufficient stock for product", exception.Message);
        Assert.Contains("Limited Item", exception.Message);
    }

}

