using Application.UnitTests.MockExtensions;
using Moq;
using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Models;
using Order_Management.Application.Orders.Commands.CreateOrder;
using Order_Management.Domain.Entities;

namespace Application.UnitTests.ValidatorTests;

public class CreateOrderCommandValidatorTests
{
    [Fact]
    public async Task Validator_ShouldReturnError_WhenProductIsInactive()
    {
        // Arrange
        var productId = 14;
        var mockProducts = new List<Product>
        {
            new Product
            {
                Id = productId,
                Name = "Insulated Lunch Box",
                Price = 16.99m,
                StockQuantity = 10,
                IsActive = false 
            }
        };

        var mockDbSet = mockProducts.ToMockDbSet();

        var mockContext = new Mock<IApplicationDbContext>();
        mockContext.Setup(c => c.Products).Returns(mockDbSet.Object);

        var validator = new CreateOrderCommandValidator(mockContext.Object);

        var command = new CreateOrderCommand
        {
            CustomerName = "Jane",
            CustomerEmail = "jane@example.com",
            OrderItems = new List<CreateOrderItemDto>
        {
            new CreateOrderItemDto
            {
                ProductId = productId,
                Quantity = 1
            }
        }
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.ErrorMessage.Contains("not available for ordering", StringComparison.OrdinalIgnoreCase));
    }
}

