namespace Order_Management.Application.Common.Response;
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string[]? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> MakeSuccess(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> MakeObject(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> MakeError(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = null
        };
    }

    public static ApiResponse<T> MakeError(string message, string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = [error]
        };
    }

    public static ApiResponse<T> MakeValidationError(string message, IDictionary<string, string[]> validationErrors)
    {
        var flatErrors = validationErrors
            .SelectMany(kvp => kvp.Value.Select(error => $"{kvp.Key}: {error}"))
            .ToArray();

        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = flatErrors,
        };
    }
    
    public static ApiResponse<T> MakeDefault()
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = default(T)
        };
    }
}
