using PhotoScavengerHunt.Exceptions;
using System.Net;
using System.Text.Json;

namespace PhotoScavengerHunt.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly string _logFilePath;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        
        var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        if (!Directory.Exists(logDirectory))
            Directory.CreateDirectory(logDirectory);
        
        _logFilePath = Path.Combine(logDirectory, "error_log.txt");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            Error = exception.Message,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case ValidationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                LogToFile(exception, context.Request.Path);
                _logger.LogWarning(exception, "Validation error: {Message}", exception.Message);
                break;

            case EntityNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                LogToFile(exception, context.Request.Path);
                _logger.LogWarning(exception, "Not found error: {Message}", exception.Message);
                break;

            case LimitExceededException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                LogToFile(exception, context.Request.Path);
                _logger.LogWarning(exception, "Limit error: {Message}", exception.Message);
                break;

            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                LogToFile(exception, context.Request.Path);
                _logger.LogWarning(exception, "Argument error: {Message}", exception.Message);
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                LogToFile(exception, context.Request.Path);
                _logger.LogWarning(exception, "Key not found: {Message}", exception.Message);
                break;

            case InvalidOperationException:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Error = "An unexpected error occurred. Please try again later.";
                LogToFile(exception, context.Request.Path);
                _logger.LogError(exception, "Invalid operation: {Message}", exception.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Error = "An unexpected error occurred. Please try again later.";
                LogToFile(exception, context.Request.Path);
                _logger.LogError(exception, "Unexpected error: {Message}", exception.Message);
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }

    private void LogToFile(Exception ex, string path)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            var message = $"[{timestamp}] {ex.GetType().Name}: {ex.Message} [Path: {path}]{Environment.NewLine}";
            
            if (ex is not (ValidationException or EntityNotFoundException or LimitExceededException or ArgumentException))
            {
                message += $"Stack Trace: {ex.StackTrace}{Environment.NewLine}{Environment.NewLine}";
            }
            
            File.AppendAllText(_logFilePath, message);
        }
        catch
        {
        }
    }

    private class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}

