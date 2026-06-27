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
            _logger.LogError(
                ex,
                "Unhandled exception occurred");

            await HandleExceptionAsync(
                context,
                ex);
        }
    }

    private static async Task HandleExceptionAsync(
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
                    response.StatusCode =  (int)HttpStatusCode.Unauthorized;
                    response.Message = exception.Message;
                    break;
            
            case NotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = exception.Message;
                break;

            case ConflictException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Message = exception.Message;
                break;

            case ValidationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                break;
            
            case BusinessRuleException:
                response.StatusCode =  (int)HttpStatusCode.UnprocessableEntity;
                response.Message = exception.Message;
                break;
            
            case BadRequestException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An unexpected error occurred.";
                break;
        }

        context.Response.StatusCode = response.StatusCode;

        await context.Response.WriteAsJsonAsync(response);
    }
}