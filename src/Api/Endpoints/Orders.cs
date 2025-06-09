using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Order_Management.Application.Common.Models;
using Order_Management.Application.Common.Response;
using Order_Management.Application.Orders.Commands.CreateOrder;
using Order_Management.Application.Orders.Commands.DeleteOrder;
using Order_Management.Application.Orders.Commands.UpdateOrder;
using Order_Management.Application.Orders.Queries.GetOrder;
using Order_Management.Application.Orders.Queries.GetOrdersWithPagination;

namespace Order_Management.Api.Endpoints;

/// <summary>
/// Controller for managing orders in the order management system.
/// Provides endpoints for creating, reading, updating, and deleting orders with pagination support.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class OrdersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<OrderSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PaginatedList<OrderSummaryDto>>>> GetOrdersWithPagination([FromQuery] GetOrdersQueryWithPagination query)
    {
        var result = await sender.Send(query);

        return ApiResponse<PaginatedList<OrderSummaryDto>>.MakeObject(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<OrderSummaryDto>>> GetOrder([FromRoute] int id)
    {
        var result = await sender.Send(new GetOrderQuery { OrderId = id });

        return ApiResponse<OrderSummaryDto>.MakeObject(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderSummaryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes("application/json")]
    public async Task<ActionResult<ApiResponse<OrderSummaryDto>>> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var result = await sender.Send(command);

        return ApiResponse<OrderSummaryDto>.MakeObject(result, "Order created successfully");
    }


    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Consumes("application/json")]
    public async Task<ActionResult<ApiResponse<OrderSummaryDto>>> UpdateOrder([FromRoute] int id, [FromBody] UpdateOrderCommand command)
    {
        command.OrderId = id;

        var result = await sender.Send(command);

        return ApiResponse<OrderSummaryDto>.MakeObject(result, "Order updated successfully");
    }

   
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ApiResponse<NoContent>> DeleteOrder([FromRoute] int id)
    {
        await sender.Send(new DeleteOrderCommand { OrderId = id });

        return ApiResponse<NoContent>.MakeDefault();
    }
}

