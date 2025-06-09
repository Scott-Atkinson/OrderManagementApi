using NotFoundException = Order_Management.Application.Common.Exceptions.NotFoundException;
using Microsoft.AspNetCore.Diagnostics;
using Order_Management.Application.Common.Exceptions;
using Order_Management.Application.Common.Response;
using System.Net;
using System.Text.Json;

namespace Order_Management.Api.Handler;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception Type: {ExceptionType}, Message: {Message}",
            exception.GetType().Name, exception.Message);

        try
        {
            var (statusCode, apiResponse) = exception switch
            {
                ValidationException ex => HandleValidationException(ex),
                NotFoundException ex => HandleNotFoundException(ex),
                ForbiddenAccessException ex => HandleForbiddenException(ex),
                BadRequestException ex => HandleBadRequestException(ex),
                ConflictException ex => HandleConflictException(ex),
                UnauthorizedAccessException => HandleUnauthorizedException(),
                InvalidOperationException ex => HandleInvalidOperationException(ex),
                _ => HandleGenericException(exception)
            };

            logger.LogInformation("Returning status code: {StatusCode} for exception: {ExceptionType}",
                (int)statusCode, exception.GetType().Name);

            httpContext.Response.StatusCode = (int)statusCode;
            httpContext.Response.ContentType = "application/json";

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = environment.IsDevelopment()
            };

            var jsonResponse = JsonSerializer.Serialize(apiResponse, jsonOptions);

            logger.LogDebug("Response JSON: {Response}", jsonResponse);

            await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);

            return true; 
        }
        catch (Exception handlerException)
        {
            logger.LogError(handlerException, "Exception handler itself failed");
            return false; 
        }
    }

    private (HttpStatusCode, ApiResponse<object>) HandleValidationException(ValidationException ex)
    {
        logger.LogInformation("Handling ValidationException with {ErrorCount} errors", ex.Errors.Count);

        var response = ApiResponse<object>.MakeValidationError("Validation failed", ex.Errors);
        return (HttpStatusCode.BadRequest, response); 
    }

    private (HttpStatusCode, ApiResponse<object>) HandleInvalidOperationException(InvalidOperationException ex)
    {
        logger.LogInformation("Handling InvalidOperationException: {Message}", ex.Message);

        var response = ApiResponse<object>.MakeError("Invalid operation", ex.Message);
        return (HttpStatusCode.BadRequest, response);
    }

    private (HttpStatusCode, ApiResponse<object>) HandleNotFoundException(NotFoundException ex)
    {
        logger.LogInformation("Handling NotFoundException: {Message}", ex.Message);

        var response = ApiResponse<object>.MakeError(ex.Message);
        return (HttpStatusCode.NotFound, response);
    }

    private (HttpStatusCode, ApiResponse<object>) HandleForbiddenException(ForbiddenAccessException ex)
    {
        var response = ApiResponse<object>.MakeError(ex.Message);
        return (HttpStatusCode.Forbidden, response);
    }

    private (HttpStatusCode, ApiResponse<object>) HandleBadRequestException(BadRequestException ex)
    {
        var response = ApiResponse<object>.MakeError(ex.Message);
        return (HttpStatusCode.BadRequest, response);
    }

    private (HttpStatusCode, ApiResponse<object>) HandleConflictException(ConflictException ex)
    {
        var response = ApiResponse<object>.MakeError(ex.Message);
        return (HttpStatusCode.Conflict, response);
    }

    private (HttpStatusCode, ApiResponse<object>) HandleUnauthorizedException()
    {
        var response = ApiResponse<object>.MakeError("Authentication required");
        return (HttpStatusCode.Unauthorized, response);
    }

    private (HttpStatusCode, ApiResponse<object>) HandleGenericException(Exception ex)
    {
        var message = environment.IsDevelopment()
            ? ex.Message
            : "An internal server error occurred";

        var response = ApiResponse<object>.MakeError("Internal server error", message);
        return (HttpStatusCode.InternalServerError, response);
    }
}

