using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Order_Management.Application.Common.Models;
using Order_Management.Application.Common.Response;
using Order_Management.Application.Products.Commands.CreateProduct;
using Order_Management.Application.Products.Commands.DeleteProduct;
using Order_Management.Application.Products.Commands.UpdateProduct;
using Order_Management.Application.Products.Queries.GetProduct;

namespace Order_Management.Api.Endpoints;

/// <summary>
/// Controller for managing products in the order management system.
/// Provides endpoints for creating, reading, updating, and deleting products.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ProductsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [Consumes("application/json")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductCommand command)
    {
        var result = await sender.Send(command);

        return ApiResponse<ProductDto>.MakeObject(result, "Product created successfully");
       
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [Consumes("application/json")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct([FromRoute] int id, [FromBody] UpdateProductCommand command) {
        
        command.Id = id;

        var result = await sender.Send(command);

        return ApiResponse<ProductDto>.MakeObject(result, "Product updated successfully");
    }


    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<NoContent>> DeleteProduct([FromRoute] int id)
    {
        await sender.Send(new DeleteProductCommand { ProductId = id });

        return ApiResponse<NoContent>.MakeDefault();
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct([FromRoute] int id)
    {
        var query = new GetProductQuery { ProductId = id };

        var result = await sender.Send(query);

        return ApiResponse<ProductDto>.MakeObject(result);

    }
}
