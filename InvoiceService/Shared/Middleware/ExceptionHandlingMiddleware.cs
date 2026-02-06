using System.Net;
using System.Text.Json;
using InvoiceService.Shared.Exceptions;

namespace InvoiceService.Shared.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
        var (statusCode, response) = exception switch
        {
            NotFoundException notFound => (
                HttpStatusCode.NotFound,
                new ErrorResponse("Not Found", notFound.Message)
            ),
            ApiValidationException validation => (
                HttpStatusCode.BadRequest,
                new ErrorResponse("Validation Error", validation.Message, validation.Errors)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse("Internal Server Error", "An unexpected error occurred.")
            )
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "An unhandled exception occurred");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

public record ErrorResponse(
    string Title,
    string Detail,
    IDictionary<string, string[]>? Errors = null);
