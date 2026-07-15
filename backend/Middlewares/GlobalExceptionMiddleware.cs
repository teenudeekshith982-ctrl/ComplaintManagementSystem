using System.ComponentModel.DataAnnotations;

namespace ComplaintManagementSystem.Middlewares;

using System.Net;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Exceptions;


public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
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

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {   
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = exception.Message;
                _logger.LogWarning(exception, "Unauthorized access: {Message}", exception.Message);
                break;
            
            case NotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = exception.Message;
                _logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
                break;

            case ConflictException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Message = exception.Message;
                _logger.LogWarning(exception, "Conflict: {Message}", exception.Message);
                break;

            case ValidationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                _logger.LogWarning(exception, "Validation error: {Message}", exception.Message);
                break;
            
            case BusinessRuleException:
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                response.Message = exception.Message;
                _logger.LogWarning(exception, "Business rule violation: {Message}", exception.Message);
                break;
            
            case BadRequestException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                _logger.LogWarning(exception, "Bad request: {Message}", exception.Message);
                break;

            default:
                var correlationId = Guid.NewGuid().ToString();
                _logger.LogError(exception, "Unhandled exception occurred. Correlation ID: {CorrelationId}", correlationId);
                
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An unexpected server error occurred. Please contact support and reference Correlation ID: " + correlationId;
                response.CorrelationId = correlationId;
                break;
        }

        context.Response.StatusCode = response.StatusCode;

        await context.Response.WriteAsJsonAsync(response);
    }
}